using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADLCore.Ext
{
    [Obsolete]
    public static class mshtml
    {
        public static IEnumerator<HtmlNode> FindAllNodes(this HtmlNode docu)
        {
            HtmlNode nn = docu;
            List<HtmlNode> HtmlNodes = new List<HtmlNode>();

            /*void cn(HtmlNode n)
            {
                foreach (HtmlNode node in n.ChildNodes)
                    if (node.HasChildNodes)
                        cn(n);
                    else
                        HtmlNodes.Add(node);
                HtmlNodes.Add(n);
            }

            foreach (HtmlNode n in docu.ChildNodes)
                cn(n);*/

            HtmlNodeCollection arr = docu.SelectNodes("//*");
            if (arr == null || arr?.Count == 0)
                return null;
            HtmlNodes.AddRange(arr);

            return HtmlNodes.GetEnumerator();
        }

        public static HtmlNode GetFirstElementByClassNameA(this IEnumerator<HtmlNode> enuma, string className)
        {
            enuma.Reset();
            while (enuma.MoveNext())
                if (enuma.Current.HasClass(className))
                    return enuma.Current;
            return null;
        }

        public static Dictionary<string, LinkedList<HtmlNode>> GetElementsByClassNames(this IEnumerator<HtmlNode> enuma,
            string[] className)
        {
            Dictionary<string, LinkedList<HtmlNode>> dict = new Dictionary<string, LinkedList<HtmlNode>>();
            LinkedList<HtmlNode>[] ihList = new LinkedList<HtmlNode>[className.Length];
            for (int idx = 0; idx < className.Length; idx++)
                ihList[idx] = new LinkedList<HtmlNode>();
            while (enuma.MoveNext())
            {
                HtmlNode ih = enuma.Current;
                for (int idx = 0; idx < className.Length; idx++)
                    if (ih.HasClass(className[idx]))
                        ihList[idx].AddLast(ih);
            }

            for (int idx = 0; idx < className.Length; idx++)
                dict.Add(className[idx], ihList[idx]);

            GC.Collect();
            return dict;
        }

        /*public static MSHTML.IHTMLElement GetFirstElementByClassNameA(this System.Collections.IEnumerator enuma, string className)
            => (enuma.MoveNext() == true) ? (((MSHTML.IHTMLElement)(enuma.Current)).className == className) ? (MSHTML.IHTMLElement)(enuma.Current) : GetFirstElementByClassName(enuma, className) : (null);

        public static MSHTML.IHTMLElement GetFirstElementByClassName(this System.Collections.IEnumerator enuma, string className)
        {
            enuma.Reset();
            while (enuma.MoveNext())
                if (((MSHTML.IHTMLElement)(enuma.Current)).className == className)
                    return (MSHTML.IHTMLElement)enuma.Current;
            return null;
        }

        public static Dictionary<string, LinkedList<MSHTML.IHTMLElement>> GetElementsByClassNames(this System.Collections.IEnumerator enuma, string[] className)
        {
            Dictionary<string, LinkedList<MSHTML.IHTMLElement>> dict = new Dictionary<string, LinkedList<MSHTML.IHTMLElement>>();
            LinkedList<IHTMLElement>[] ihList = new LinkedList<IHTMLElement>[className.Length];
            for (int idx = 0; idx < className.Length; idx++)
                ihList[idx] = new LinkedList<IHTMLElement>();
            while (enuma.MoveNext())
            {
                IHTMLElement ih = (IHTMLElement)enuma.Current;
                for (int idx = 0; idx < className.Length; idx++)
                    if (ih.className == className[idx])
                    {
                        Console.WriteLine(ih.className);
                        ihList[idx].AddLast(ih);
                    }
            }
            for (int idx = 0; idx < className.Length; idx++)
                dict.Add(className[idx], ihList[idx]);

            GC.Collect();
            return dict;
        }

        public static List<IHTMLElement> getLiElements(this MSHTML.IHTMLElement div)
        {
            List<IHTMLElement> el = new List<IHTMLElement>();
            foreach (IHTMLElement ele in (IHTMLElementCollection)div.children)
                if (ele.tagName == "LI" || ele.tagName == "li")
                    el.Add(ele);
            return el;
        }

        public static MSHTML.IHTMLDocument2 WriteSafeDocument(this MSHTML.IHTMLDocument2 doc, string toWrite)
        {
            if (doc == null)
                doc = GetDefaultDocument();

            doc.designMode = "On";
            doc.clear();
            doc.write(toWrite);
            doc.close();
            return doc;
        }

        public static MSHTML.IHTMLDocument2 GetDefaultDocument()
            => new HTMLDocument() as IHTMLDocument2;
        public static MSHTML.IHTMLDocument2 GetDefaultDocument(this MSHTML.IHTMLDocument2 doc)
            => new HTMLDocument() as IHTMLDocument2;*/
    }
}