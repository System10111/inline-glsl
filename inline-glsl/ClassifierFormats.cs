using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace inline_glsl
{
    internal static class ShaderClassifierClassificationDefinition
    {
#pragma warning disable 169
        /*
            Punctuation                     = white
            PeekLabelText                   = white
            Character                       = white
            Literal                         = lighter gray
            MarkupAttributeValue            = light gray
            Operator                        = light gray
            PreprocessorKeyword             = gray
            ExcludedCode                    = gray
            SymbolReference                 = cyan
            MarkupAttribute                 = lighter blue
            Type                            = light blue
            SymbolDefinition                = blue
            MarkupNode                      = blue
            Keyword                         = blue
            PeekFocusedBorder               = dark blue
            Number                          = light green
            Comment                         = green
            String                          = orange
            Other                           = x
            Text                            = x
            WhiteSpace                      = x
            Identifier                      = x
            PeekBackgroundUnfocused         = x
            NaturalLanguage                 = x
            FormalLanguage                  = x
            PeekHighlightedTextUnfocused    = orange background
            PeekHistoryHovered              = light blue background
            PeekHighlightedText             = blue background
            PeekHistorySelected             = light blue background
            PeekBackground                  = blue background

         */


        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderKeywords")]
        [BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
        private static ClassificationTypeDefinition shaderKeywords;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderControlKeywords")]
        [BaseDefinition("keyword - control")]
        private static ClassificationTypeDefinition shaderControlKeywords;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderPreprocessorKeywords")]
        [BaseDefinition(PredefinedClassificationTypeNames.PreprocessorKeyword)]
        private static ClassificationTypeDefinition shaderPreprocessorKeywords;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderComments")]
        [BaseDefinition(PredefinedClassificationTypeNames.Comment)]
        private static ClassificationTypeDefinition shaderComments;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderIdentifiers1")]
        [BaseDefinition(PredefinedClassificationTypeNames.MarkupAttribute)]
        private static ClassificationTypeDefinition shaderIdentifiers1;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderIdentifiers2")]
        [BaseDefinition(PredefinedClassificationTypeNames.Literal)]
        private static ClassificationTypeDefinition shaderIdentifiers2;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderIdentifiers3")]
        [BaseDefinition(PredefinedClassificationTypeNames.SymbolReference)]
        private static ClassificationTypeDefinition shaderIdentifiers3;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderOperators")]
        [BaseDefinition(PredefinedClassificationTypeNames.Operator)]
        private static ClassificationTypeDefinition shaderOperators;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderNumbers")]
        [BaseDefinition(PredefinedClassificationTypeNames.Number)]
        private static ClassificationTypeDefinition shaderNumbers;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderExcludedCode")]
        [BaseDefinition(PredefinedClassificationTypeNames.ExcludedCode)]
        private static ClassificationTypeDefinition shaderExcludedCode;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderText")]
        [BaseDefinition("punctuation")]
        private static ClassificationTypeDefinition shaderText;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderType")]
        [BaseDefinition(PredefinedClassificationTypeNames.Type)]
        private static ClassificationTypeDefinition shaderType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ShaderFunctions")]
        [BaseDefinition("cppFunction")]
        private static ClassificationTypeDefinition shaderFunctions;

#pragma warning restore 169
    }
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ShaderKeywords")]
    [ClassificationType(ClassificationTypeNames = "ShaderControlKeywords")]
    [ClassificationType(ClassificationTypeNames = "ShaderPreprocessorKeywords")]
    [ClassificationType(ClassificationTypeNames = "ShaderComments")]
    [ClassificationType(ClassificationTypeNames = "ShaderIdentifiers1")]
    [ClassificationType(ClassificationTypeNames = "ShaderIdentifiers2")]
    [ClassificationType(ClassificationTypeNames = "ShaderIdentifiers3")]
    [ClassificationType(ClassificationTypeNames = "ShaderOperators")]
    [ClassificationType(ClassificationTypeNames = "ShaderNumbers")]
    [ClassificationType(ClassificationTypeNames = "ShaderExcludedCode")]
    [ClassificationType(ClassificationTypeNames = "ShaderText")]
    [ClassificationType(ClassificationTypeNames = "ShaderType")]
    [ClassificationType(ClassificationTypeNames = "ShaderFunctions")]
    [Name("ShaderClassifierFormat")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(After = DefaultOrderings.Highest)] // Set the priority to be the highest, because we'll be overwriting a string literal
    internal sealed class ShaderClassifierFormat : ClassificationFormatDefinition
    {
        public ShaderClassifierFormat()
        {
            this.DisplayName = "ShaderClassifierFormat"; // Human readable version of the name
        }
    }

}
