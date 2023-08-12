using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Hibzz.Singletons.Editor
{
    public class StaticAccess : AssetPostprocessor
    {
        const string STATIC_ACCESS_PAUSE_KEY = "Hibzz/Singletons/Pause Automatic Code Generation";

        // gets a notification after all asset has been imported
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            // if the user wants to pause the automatic code generation of this system, dont proceed
            if(Menu.GetChecked(STATIC_ACCESS_PAUSE_KEY)) { return; }

            // flag that indicates if code has been generated
            bool codeGenerated = false;

            // generate code for each imported asset if it's valid
            foreach (var importedAsset in importedAssets)
            {
                if (TryGenerateCode(importedAsset))
                {
                    codeGenerated = true;
                }
            }

            // reload assembly if code was generated (or removed)
            if (codeGenerated)
            {
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Tries to generate any singleton related code for the given asset
        /// </summary>
        /// <param name="assetPath">The path to the script asset to analyze and generate code for</param>
        /// <returns>Was some code generated for the given asset?</returns>
        private static bool TryGenerateCode(string assetPath)
        {
            // if the changed asset is not a script file, we don't care
            if (!assetPath.EndsWith(".cs")) { return false; }

            // Issue: For now this is okay, but in the future this decision will cause issues,
            // for example what happens the user generates a singleton script that has the static
            // access attribute in it? The system will see that the file name contains the common
            // convention of .generated.cs so it'll ignore it. Alternatively we generate files
            // that end with .staticaccess.generated.cs but that's too long. We'll cross that
            // bridge when the time comes, but just jotting this down so it's easier to understand
            // the issue later.

            // if the generated script file is a generated script file, we don't care either
            if (assetPath.EndsWith(".generated.cs")) { return false; }

            // load the asset from the given path as a monoscript
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            if (monoScript is null) { return false; }

            // make sure that they one of the variants of the singleton classes provided
            var classType = monoScript.GetClass();
            if (classType is null) { return false; }
            if (!classType.IsASingletonVariant()) { return false; }

            // don't generate code for abstract classes
            if (classType.IsAbstract) { return false; }

            // whether we generate some new content or not, we are going to need the path to the
            // expected generated asset... at the end of this process we either create a new one,
            // or check and delete an existing one
            string generatedAssetPath = assetPath;
            generatedAssetPath = generatedAssetPath.Substring(0, generatedAssetPath.LastIndexOf(".cs"));
            generatedAssetPath += ".generated.cs";

            // Now we need to generate the script file
            if (GetGeneratedCode(classType, out string code))
            {
                File.WriteAllText(generatedAssetPath, code);
            }
            else 
            {
                // no code was generated, so remove any existing generated file
                if(!File.Exists(generatedAssetPath)) { return false; }
                File.Delete(generatedAssetPath);
            }

            return true;
        }

        /// <summary>
        /// Get the generated code from the given type
        /// </summary>
        /// <param name="classType">The type to analyze</param>
        /// <param name="code">The generated code as a string</param>
        /// <returns>Was code generated as a string</returns>
        private static bool GetGeneratedCode(Type classType, out string code)
        {
            bool wasCodeGenerated = false; // flag indicating whether some code was generated or not
            string classContent = "";      // store what the contents of the class file is

            // loop through all instanced members of the class and if it's members do contain the
            // attribute [StaticAccess] then we make add a public static accessor for that member
            var members = classType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var member in members)
            {
                // early return if no [StaticAccess] attribute found
                var staticAccessAttribute = member.GetCustomAttribute<StaticAccessAttribute>();
                if (staticAccessAttribute is null) { continue; }

                // figure out the variable name
                var variableName = staticAccessAttribute.VariableName;
                if (variableName is null)
                {
                    // since no specific variable name is given, it must be auto generated
                    variableName = member.Name;

                    // a naming scheme is specified in the attribute declaration, which requires an
                    // underscore for the auto generation to work
                    if (!variableName.Contains("_"))
                    {
                        Debug.LogWarning($"Skipped generating static accessor for {classType.Name}.{member.Name}: " +
                            $"The member name must contain some leading underscore for name to be autogenerated. " +
                            $"Please pass a string as a variable name to [StaticAccess(varname)] if you don't want " +
                            $"the name to be autogenerated.\n");

                        continue;
                    }

                    // remove anything before the first underscore and capitalize the first character
                    variableName = variableName.Substring(variableName.IndexOf('_') + 1);
                    variableName = char.ToUpper(variableName[0]) + variableName.Substring(1);
                }


                // based on the member type, the public accessor's syntax varies
                if (member.MemberType == MemberTypes.Field)
                {
                    var fieldInfo = member as FieldInfo;
                    classContent += $"public static {fieldInfo.FieldType.FullName} {variableName} => Instance.{member.Name};\n";
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    var propertyInfo = member as PropertyInfo;
                    classContent += $"public static {propertyInfo.PropertyType.FullName} {variableName} => Instance.{member.Name};\n";
                }
                else if (member.MemberType == MemberTypes.Method)
                {
                    var methodInfo = member as MethodInfo;

                    // stores any formal/actual parameters
                    string formalParameters = "";
                    string actualParameters = "";

                    // decipher what the formal and actual parameters of this function are (if any)
                    var parameters = methodInfo.GetParameters();
                    foreach(var parameter in parameters)
                    {
                        formalParameters += $"{parameter.ParameterType} {parameter.Name}, ";
                        actualParameters += $"{parameter.Name}, ";
                    }

                    // remove the trailing comma character from the parameters
                    if(parameters.Length > 0)
                    {
                        formalParameters = formalParameters.Substring(0, formalParameters.LastIndexOf(','));
                        actualParameters = actualParameters.Substring(0, actualParameters.LastIndexOf(','));
                    }

                    // string that all together
                    classContent += $"public static {methodInfo.ReturnType.FullName} {variableName}({formalParameters}) => Instance.{member.Name}({actualParameters});\n";
                }

                // update the flag letting the system know that some valid code was indeed generated
                wasCodeGenerated = true;
            }

            // when no code was generated, either because there were no [StaticAccess] attributes
            // or the user wanted to have an autogenerated variable name without a leading
            // underscore, we want to perform an early exit
            if(!wasCodeGenerated) 
            {
                code = "";
                return false; 
            }

            // stores the generated code for just the class
            string classCode = "";

            // classes can either be public or internal, and if there's no access modifier present,
            // it'll be considered to be internal
            if (classType.IsPublic)
            {
                classCode += "public ";
            }

            // add the class defenition
            classCode += $"partial class {classType.Name} : {classType.GetSingletonBaseName()} \n" +
                          "{ \n" +
                         $"{classContent}" +
                          "} \n" ;

            // the overall code that's generated needs to be put together
            code = "// this code is automatically generated by the Hibzz.Singletons package\n\n";
            code += "using Hibzz.Singletons;\n\n";
            if (!string.IsNullOrWhiteSpace(classType.Namespace))
            {
                code += $"namespace {classType.Namespace} \n" +
                         "{ \n" +
                        $"{classCode}" +
                         "} \n";
            }
            else
            {
                code += classCode;
            }

            return true; // code was indeed generated
        }

        [MenuItem(STATIC_ACCESS_PAUSE_KEY)]
        private static void OnStaticAccessPauseMenuPressed()
        {
            // this should toggle the Automatic Code Generation
            Menu.SetChecked(STATIC_ACCESS_PAUSE_KEY, !Menu.GetChecked(STATIC_ACCESS_PAUSE_KEY));
        }
    }

    internal static class TypeExtensions
    {
        /// <summary>
        /// Alternative version of <see cref="Type.IsSubclassOf"/> that supports raw generic types (generic types without
        /// any type parameters).
        /// </summary>
        /// <param name="baseType">The base type class for which the check is made.</param>
        /// <param name="toCheck">To type to determine for whether it derives from <paramref name="baseType"/>.</param>
        /// <remarks>
        /// Credits to Denis Doomen (https://github.com/dennisdoomen)
        /// </remarks>
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type baseType)
        {
            while (toCheck != typeof(object))
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (baseType == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Get the base type as a string for a singleton class
        /// </summary>
        /// <param name="type">The type of a class that inherits one of the singleton variants</param>
        /// <returns>A string representation of the base type of the singleton class</returns>
        public static string GetSingletonBaseName(this Type type)
        {
            // we are working with the generic base type

            // when we get the name of a generic base type, c# reflection systems would give us
            // something of a compiled form... like Singleton`1 but that's not useful for code
            // generation

            // This is not a general purpose function, it's internal and is very specific to
            // singletons package... Although it's possible to inherit a class that inherits the
            // Singleton base class, it's not practical. None of the current class member can be
            // accessed using the instance property

            // So, we are cutting corners and creating something very specific for the singletons
            // package that follows the pattern of base<SameType>

            // Issue: When in the future we decide to fix the inheritance issue (if it can be fixed),
            // we need to revisit this function to be general purpose

            string generic_base = type.BaseType.Name;
            generic_base = generic_base.Substring(0, generic_base.IndexOf('`'));

            return $"{generic_base}<{type.Name}>";
        }

        /// <summary>
        /// check if the given type is on off the singleton variants
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>Is the given type on off the variants of the singleton</returns>
        public static bool IsASingletonVariant(this Type type)
        {
            if (type.IsSubclassOfRawGeneric(typeof(Singleton<>))) { return true; }
            if (type.IsSubclassOfRawGeneric(typeof(ScriptableSingleton<>))) { return true; }

            return false;
        }
    }
}
