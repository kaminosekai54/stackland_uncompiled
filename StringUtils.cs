public static class StringUtils
{
	public static string RemoveRichText(string input)
	{
		input = StringUtils.RemoveRichTextDynamicTag(input, "color");
		input = StringUtils.RemoveRichTextTag(input, "b");
		input = StringUtils.RemoveRichTextTag(input, "i");
		input = StringUtils.RemoveRichTextDynamicTag(input, "align");
		input = StringUtils.RemoveRichTextDynamicTag(input, "size");
		input = StringUtils.RemoveRichTextDynamicTag(input, "cspace");
		input = StringUtils.RemoveRichTextDynamicTag(input, "font");
		input = StringUtils.RemoveRichTextDynamicTag(input, "indent");
		input = StringUtils.RemoveRichTextDynamicTag(input, "line-height");
		input = StringUtils.RemoveRichTextDynamicTag(input, "line-indent");
		input = StringUtils.RemoveRichTextDynamicTag(input, "link");
		input = StringUtils.RemoveRichTextDynamicTag(input, "margin");
		input = StringUtils.RemoveRichTextDynamicTag(input, "margin-left");
		input = StringUtils.RemoveRichTextDynamicTag(input, "margin-right");
		input = StringUtils.RemoveRichTextDynamicTag(input, "mark");
		input = StringUtils.RemoveRichTextDynamicTag(input, "mspace");
		input = StringUtils.RemoveRichTextDynamicTag(input, "noparse");
		input = StringUtils.RemoveRichTextDynamicTag(input, "nobr");
		input = StringUtils.RemoveRichTextDynamicTag(input, "page");
		input = StringUtils.RemoveRichTextDynamicTag(input, "pos");
		input = StringUtils.RemoveRichTextDynamicTag(input, "space");
		input = StringUtils.RemoveRichTextDynamicTag(input, "sprite index");
		input = StringUtils.RemoveRichTextDynamicTag(input, "sprite name");
		input = StringUtils.RemoveRichTextDynamicTag(input, "sprite");
		input = StringUtils.RemoveRichTextDynamicTag(input, "style");
		input = StringUtils.RemoveRichTextDynamicTag(input, "voffset");
		input = StringUtils.RemoveRichTextDynamicTag(input, "width");
		input = StringUtils.RemoveRichTextTag(input, "u");
		input = StringUtils.RemoveRichTextTag(input, "s");
		input = StringUtils.RemoveRichTextTag(input, "sup");
		input = StringUtils.RemoveRichTextTag(input, "sub");
		input = StringUtils.RemoveRichTextTag(input, "allcaps");
		input = StringUtils.RemoveRichTextTag(input, "smallcaps");
		input = StringUtils.RemoveRichTextTag(input, "uppercase");
		return input;
	}

	private static string RemoveRichTextDynamicTag(string input, string tag)
	{
		int num = -1;
		while (true)
		{
			num = input.IndexOf("<" + tag + "=");
			if (num == -1)
			{
				break;
			}
			int num2 = input.Substring(num, input.Length - num).IndexOf('>');
			if (num2 > 0)
			{
				input = input.Remove(num, num2 + 1);
			}
		}
		input = StringUtils.RemoveRichTextTag(input, tag, isStart: false);
		return input;
	}

	private static string RemoveRichTextTag(string input, string tag, bool isStart = true)
	{
		while (true)
		{
			int num = input.IndexOf(isStart ? ("<" + tag + ">") : ("</" + tag + ">"));
			if (num == -1)
			{
				break;
			}
			input = input.Remove(num, 2 + tag.Length + (!isStart).GetHashCode());
		}
		if (isStart)
		{
			input = StringUtils.RemoveRichTextTag(input, tag, isStart: false);
		}
		return input;
	}
}
