using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace inline_glsl
{
	/// <summary>
	/// Classifier that classifies all text as an instance of the "ShaderClassifier" classification type.
	/// </summary>
	internal class ShaderClassifier : IClassifier
	{
		/// <summary>
		/// Classification type.
		/// </summary>
		private readonly IClassificationType keywordsClassifier;
		private readonly IClassificationType preprocessorKeywordsClassifier;
		private readonly IClassificationType commentsClassifier;
		private readonly IClassificationType identifiers1Classifier;
		private readonly IClassificationType identifiers2Classifier;
		private readonly IClassificationType identifiers3Classifier;
		private readonly IClassificationType operatorsClassifier;
		private readonly IClassificationType numbersClassifier;
		private readonly IClassificationType excludedCodeClassifier;
		private readonly IClassificationType textClassifier;
		private readonly IClassificationType typeClassifier;

		public IStandardClassificationService Classifications;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShaderClassifier"/> class.
		/// </summary>
		/// <param name="registry">Classification registry.</param>
		internal ShaderClassifier(IClassificationTypeRegistryService registry, IStandardClassificationService classifications, ITextBuffer buffer)
		{
			this.keywordsClassifier = registry.GetClassificationType("ShaderKeywords");
			this.preprocessorKeywordsClassifier = registry.GetClassificationType("ShaderPreprocessorKeywords");
			this.commentsClassifier = registry.GetClassificationType("ShaderComments");
			this.identifiers1Classifier = registry.GetClassificationType("ShaderIdentifiers1");
			this.identifiers2Classifier = registry.GetClassificationType("ShaderIdentifiers2");
			this.identifiers3Classifier = registry.GetClassificationType("ShaderIdentifiers3");
			this.operatorsClassifier = registry.GetClassificationType("ShaderOperators");
			this.numbersClassifier = registry.GetClassificationType("ShaderNumbers");
			this.excludedCodeClassifier = registry.GetClassificationType("ShaderExcludedCode");
			this.textClassifier = registry.GetClassificationType("ShaderText");
			this.typeClassifier = registry.GetClassificationType("ShaderType");
			Classifications = classifications;
		}

		#region IClassifier

#pragma warning disable 67

		/// <summary>
		/// An event that occurs when the classification of a span of text has changed.
		/// </summary>
		/// <remarks>
		/// This event gets raised if a non-text change would affect the classification in some way,
		/// for example typing /* would cause the classification to change in C# without directly
		/// affecting the span.
		/// </remarks>
		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67
		public Dictionary<int, SnapshotSpan> glsl_spans = new Dictionary<int, SnapshotSpan>();
		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
		{
			string[] glsl_keywords = new string[]{ "attribute", "const", "uniform", "varying", "centroid", "break",
					"continue", "do","for","while","if","else","in","out",
					"inout","float","int","void","bool","true","false",
					"invariant","discard","return","mat2","mat3","mat4",
					"mat2x2","mat2x3","mat2x4","mat3x2","mat3x3","mat3x4",
					"mat4x2","mat4x3","mat4x4","vec2","vec3","vec4","ivec2",
					"ivec3","ivec4","bvec2","bvec3","bvec4","sampler1D",
					"sampler2D","sampler3D","samplerCube","sampler1DShadow",
					"sampler2DShadow","struct"};

			var result = new List<ClassificationSpan>();
			var line = span.Snapshot.GetLineNumberFromPosition(span.End);

			// use a comment like "// vertex shader:" to denote where glsl should be highlighted
			var comment_match = Regex.Match(
				span.GetText(),
				"\\/\\/[ \t]*(vertex|fragment|compute)[ \t]+shader[ \t]*:|\\/\\*[ \t]*(vertex|fragment|compute)[ \t]+shader[ \t]*:\\*\\/",
				RegexOptions.IgnoreCase
			);

			if (comment_match.Success)
			{
				// mark the comment as a keyword
				result.Add(new ClassificationSpan(new SnapshotSpan(span.Snapshot,
					new Span(span.Start + comment_match.Index, comment_match.Length)),
					typeClassifier
				));
				// now find the shader string
				var index = span.Start + comment_match.Index + comment_match.Length;
				// find the opening quotes
				while (span.Snapshot[index] != '"' || span.Snapshot[index - 1] == '\"')
				{
					// if following the comment is something other than a string, then discard everything
					if (!char.IsWhiteSpace(span.Snapshot[index]))
					{
						if (glsl_spans.ContainsKey(line)) glsl_spans.Remove(line);
						return new List<ClassificationSpan>();
					}
					index = index + 1;
				}
				var string_start = index + 1;
				index = index + 1; // move away from the opening quotes

				// find the closing quotes
				while (span.Snapshot[index] != '"' || span.Snapshot[index - 1] == '\"')
				{
					// non-escaped new line - discard
					// index -1 and -2 because some files' lines end with \n and some with \r\n
					if (span.Snapshot[index] == '\n' && !(span.Snapshot[index - 1] == '\\' || span.Snapshot[index - 2] == '\\'))
					{
						if (glsl_spans.ContainsKey(line)) glsl_spans.Remove(line);
						return new List<ClassificationSpan>();
					}
					index = index + 1;
				}
				var string_end = index;

				var snap_span = new SnapshotSpan(span.Snapshot, new Span(string_start, string_end - string_start));

				ClassificationChanged.Invoke(this, new ClassificationChangedEventArgs(snap_span));
				if (!glsl_spans.ContainsKey(line)) glsl_spans.Add(line, new SnapshotSpan());
				glsl_spans[line] = snap_span;
			} else if (glsl_spans.ContainsKey(line)) 
			{
				glsl_spans.Remove(line);
			}


			//passed the comment, or no comment on this line, is the span in the last of the glsl spans?
			var span_line = (from k in glsl_spans.Keys where k <= line select k).DefaultIfEmpty(-1).Max();
			if (span_line == -1) return result;

			// if the snapshot is just pretend it is correct and invalidate the comment line
			if (span.Snapshot != glsl_spans[span_line].Snapshot)
            {
				glsl_spans[span_line] = new SnapshotSpan(span.Snapshot, glsl_spans[span_line].Span);
				ClassificationChanged.Invoke(this, new ClassificationChangedEventArgs(span.Snapshot.GetLineFromLineNumber(span_line).Extent));
			}
			//calculate what is actually inside the glsl string
			var inter_or_null = span.Intersection(glsl_spans[span_line]);
			if (inter_or_null == null) return result;
			SnapshotSpan inter = (SnapshotSpan)inter_or_null;

			//make the escaped newline darker
			var escaped_index = inter.GetText().IndexOf("\\n\\");
			if (escaped_index != -1)
            {
				result.Add(new ClassificationSpan(new SnapshotSpan(inter.Start + escaped_index, 3),
					excludedCodeClassifier
				));
				inter = new SnapshotSpan(inter.Start, escaped_index);
            }

			int version = 0;
			bool comment = false;
			bool after_dot = false;
			//iterate all words
			for(var i = inter.Start; i < inter.End;)
            {
				//move away from whitespaces
				while (Char.IsWhiteSpace(i.GetChar())) i = i + 1;
				if (i >= inter.End) break;

				var end_of_token = i + 1;// new SnapshotSpan(i, inter.End).GetText().IndexOfAny(" \t".ToCharArray());
				char first_char = i.GetChar();

				IClassificationType c_type = textClassifier;

				//chars which are a token by themselves
				if ("-.,;[](){}".Contains(first_char))
				{
					if (first_char == '.')
                    {
						after_dot = true;
                    }
					c_type = operatorsClassifier;
					end_of_token = i + 1;
				}
				else if ("0123456789".Contains(first_char))
				{
					//numbers
					while (end_of_token < inter.End && "0123456789.".Contains(end_of_token.GetChar())) 
						end_of_token = end_of_token + 1;
					c_type = numbersClassifier;
				}
				else if ("_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(first_char))
				{
					//identifiers
					while (end_of_token < inter.End && "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Contains(end_of_token.GetChar())) 
						end_of_token = end_of_token + 1;
					if(after_dot)
                    {
						after_dot = false;
						c_type = identifiers2Classifier;
                    } else
                    {
						c_type = identifiers1Classifier;
                    }
				}
				if (first_char == '#')
				{
					//identifiers
					while (end_of_token < inter.End && "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Contains(end_of_token.GetChar())) 
						end_of_token = end_of_token + 1;
					c_type = preprocessorKeywordsClassifier;
				} else if(first_char == '/' && (i+1).GetChar() == '/')
                {
					comment = true;
				}
				var word = new SnapshotSpan(i, end_of_token);

				if(comment)
                {
					c_type = commentsClassifier;
                } else if (word.GetText().StartsWith("#version"))
                {
					c_type = preprocessorKeywordsClassifier;
					version = 1;
                } else if(version == 1)
                {
					c_type = numbersClassifier;
					version = 2;
				} else if (version == 2)
                {
					c_type = textClassifier;
					version = 0;
				} else if (glsl_keywords.Contains(word.GetText()))
                {
					c_type = keywordsClassifier;
				} else if (word.GetText().StartsWith("gl_"))
                {
					c_type = identifiers3Classifier;
                }


				result.Add(new ClassificationSpan(word,
					c_type
				));
				i = end_of_token;
            }


			return result;
		}

		#endregion
	}
}
