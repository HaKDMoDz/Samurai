﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Samurai.Content.Pipeline
{
	public class ContentProjectSerializeNode
	{
		public string FileName { get; set; }

		public Dictionary<string, string> Parameters { get; set; }

		public ContentProjectSerializeNode()
		{
			this.Parameters = new Dictionary<string, string>();
		}

		internal static ContentProjectSerializeNode FromXElement(XElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			ContentProjectSerializeNode serializer = new ContentProjectSerializeNode();

			serializer.FileName = element.GetRequiredAttributeValue("FileName");

			foreach (XAttribute attribute in element.Attributes().Where(x => x.Name != "FileName"))
			{
				serializer.Parameters[attribute.Name.ToString()] = attribute.Value;
			}

			return serializer;
		}

		internal object Build(ContentProjectContext context, object content)
		{
			context.Logger.BeginSection(string.Format("Serialize: {0}", this.FileName));
						
			var serializer = context.GetContentSerializer(content.GetType());

			ReflectionHelper.ApplyParameters(context, serializer, this.Parameters);

			using (ContentWriter writer = new ContentWriter(this.FileName))
			{
				serializer.Serialize(content, writer);
			}

			context.Logger.EndSection();

			return content;
		}
	}
}