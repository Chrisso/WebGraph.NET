
function getPluginTitle()
{
    return "Wikipedia EN (Xml-Export)";
}

function getContentUrl(keyword)
{
    return "http://en.wikipedia.org/wiki/Special:Export/" 
        + encodeURIComponent(keyword.replace(/\s/g, "_"));
}

function getKeywords(data)
{
    var result = new Array();
    var worker = data.toString();

    var text_start = worker.indexOf("<text xml:space=\"preserve\">");
    if (text_start != -1)       // throw surrounding xml away
        worker = worker.substring(text_start + 27, worker.indexOf("</text>"));        

    var article_start = worker.indexOf("==");
    if (article_start != -1)    // throw main article away and process teaser only
        worker = worker.substring(0, article_start);  

    var link_end = -1;
    var link_start = worker.indexOf("[[");
    while (link_start != -1)
    {
        link_end = worker.indexOf("]]", link_start);
        if (link_end != -1)
        {
            var keyword = worker.substring(link_start + 2, link_end);
            if (keyword.indexOf("|") != -1)     // is there an alias?
                keyword = keyword.substring(0, keyword.indexOf("|"));
            if (keyword.indexOf(":") == -1 && keyword.indexOf("#") == -1)   // only non-special links
                result.push(keyword);
        }
        link_start = worker.indexOf("[[", link_start + 2);
    }

    return result;
}

function getTeaser(data)
{
    var result = new Array();
    var worker = data.toString();

    var text_start = worker.indexOf("<text xml:space=\"preserve\">");
    if (text_start != -1)       // throw surrounding xml away
        worker = worker.substring(text_start + 27, worker.indexOf("</text>"));

    var article_start = worker.indexOf("==");
    if (article_start != -1)    // throw main article away and process teaser only
        worker = worker.substring(0, article_start);

    worker = worker.replace(/\n/g, " ");
    worker = worker.replace(/\&amp;nbsp;/g, " ");
    worker = worker.replace(/\s+/g, " ");
    worker = worker.replace(/\&lt;\!\-\-.+?\-\-\&gt;/g, "");    // <!-- comment -->
    worker = worker.replace(/\{\{\w+?\}\}/g, "");       // {{template}}
    worker = worker.replace(/\{\{.+?\}\}/g, "");        // {{template}}
    worker = worker.replace(/\{\|.+?\|\}/g, "");        // {{table}}
    worker = worker.replace(/\&lt;ref\&gt;.+?\&lt;\/ref\&gt;/g, "");    // <ref>...</ref>
    worker = worker.replace(/\&lt;pre\&gt;.+?\&lt;\/pre\&gt;/g, "");    // <pre>...</pre>
    worker = worker.replace(/\&lt;sub\&gt;.+?\&lt;\/sub\&gt;/g, "");    // <sub>...</sub>
    worker = worker.replace(/\&lt;sup\&gt;.+?\&lt;\/sup\&gt;/g, "");    // <sup>...</sup>
    worker = worker.replace(/\'{3,5}(.+?)\'{3,5}/g, "$1");              // '''text'''
    worker = worker.replace(/\={2,5}(.+?)\={2,5}/g, "$1");              // ==text==
    worker = worker.replace(/\[\[[^\]]+?\:[^\]]+?\|(.+?)\]\]/g, "$1");  // [[special:link|alias]]
    worker = worker.replace(/\[\[[^\]]+?\|(.+?)\]\]/g, "$1");           // [[link|alias]]
    worker = worker.replace(/\[\[(.+?)\]\]/g, "$1");                    // [[link]]

    return worker.replace(/^\s+/, "").replace(/\s+$/, "");
}
