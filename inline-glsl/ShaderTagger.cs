using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Windows.Documents;
using System.Linq;

namespace inline_glsl
{
	internal class ShaderTagger : ITagger<ErrorTag>
	{
		ITextView view { get; set; }
		ITextBuffer sourceBuffer { get; set; }
		ShaderClassifier classifier { get; set; }

		public ShaderTagger(ITextView view, ITextBuffer sourceBuffer, IClassifier classifier)
		{
			this.view = view;
			this.sourceBuffer = sourceBuffer;
			this.classifier = (ShaderClassifier)classifier;
			this.classifier.tagger = this;
		}

		public void raiseTagsChanged(SnapshotSpanEventArgs args)
		{
			TagsChanged?.Invoke(this, args);
		}

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
		public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			if (classifier == null) return null;
			var res = new List<ITagSpan<ErrorTag>>();

			foreach (var span in spans)
			{
				//the line on which the current span is
				var line = span.Snapshot.GetLineNumberFromPosition(span.End);

				var span_line = (from k in classifier.glsl_strings.Keys where k <= line select k).DefaultIfEmpty(-1).Max();
				if (span_line == -1) continue;

				var errors = classifier.glsl_strings[span_line].errors;
				foreach(var (err_span, err_msg) in errors)
                {
					var intr = err_span.GetSpan(span.Snapshot).Intersection(span);
					if (!intr.HasValue) continue;
					res.Add(new TagSpan<ErrorTag>(intr.Value, new ErrorTag("Shader error", err_msg)));
                }
			}

			return res;
		}
	}
}