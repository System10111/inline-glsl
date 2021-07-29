using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace inline_glsl
{
    /// <summary>
    /// Classifier provider. It adds the classifier to the set of classifiers.
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ErrorTag))]
    [Export(typeof(IClassifierProvider))]
    [ContentType("C/C++")] // This classifier applies to all text files.
    internal class ShaderClassifierProvider : IClassifierProvider, IViewTaggerProvider
    {
        // Disable "Field is never assigned to..." compiler's warning. Justification: the field is assigned by MEF.
#pragma warning disable 649

        /// <summary>
        /// Classification registry to be used for getting a reference
        /// to the custom classification type later.
        /// </summary>
        [Import]
        private IClassificationTypeRegistryService classificationRegistry;
        [Import]
        private IStandardClassificationService classifications = null;
#pragma warning restore 649

        #region IClassifierProvider


        Dictionary<ITextBuffer, IClassifier> classifiers = new Dictionary<ITextBuffer, IClassifier>();

        /// <summary>
        /// Gets a classifier for the given text buffer.
        /// </summary>
        /// <param name="buffer">The <see cref="ITextBuffer"/> to classify.</param>
        /// <returns>A classifier for the text buffer, or null if the provider cannot do so in its current state.</returns>
        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            var res = buffer.Properties.GetOrCreateSingletonProperty<ShaderClassifier>(
                creator: () => new ShaderClassifier(this.classificationRegistry, classifications, buffer
            ));
            if(classifiers.ContainsKey(buffer))
                classifiers[buffer] = res;
            else
                classifiers.Add(buffer, res);
            return res;
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            //provide highlighting only on the top buffer 
            if (textView.TextBuffer != buffer)
                return null;
            return new ShaderTagger(textView, buffer, classifiers[buffer]) as ITagger<T>;
        }

        #endregion
    }
}