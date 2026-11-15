namespace ClipX2.TextEngine
{
    internal static class ClipXConstants
    {
        public const string Quotes = "\"'‘’“”";
        public const string BracketsOpen = "([{<";
        public const string BracketsClose = ")]}>";
        public const string SentenceTerminators = ".?!";
        public const string Ellipsis = "...…";
        public const string PUNCTUATION = ".,?!;:";
        public const string HYPHENATED_PREFIXES = ",ante,anti,bi,bio,co,counter,de,extra,infra,inter,intra,macro,meta,micro,mid,mini,multi,neo,non,off,over,post,pre,pro,proto,pseudo,re,semi,socio,sub,super,supra,trans,quasi,ultra,un,under,";
        public const string WORDS_NOT_TITLE_CASE = ",a,an,and,as,at,but,by,else,for,from,if,in,into,nor,not,of,off,on,or,out,over,so,the,then,to,until,up,v,vs,when,where,with,yet,";
        // Omitted because long: across, after, because, before, beneath, since, under
        // Omitted verb 'to be':am,are,be,is,was,were
        // Use initial capital letters for subordinate conjunctions (after, although, as, as if, because, before, if, since, unless, when, while)
        // Whole words only (matched with IsInList, so a leading and trailing comma are needed). Plurals and possessives are handled in code.
        public const string WORDS_SECOND_CHAR_CAP = ",iphone,ipad,ipod,itunes,ios,imac,icloud,imovie,iphoto,imessage,eharmony,etrade,emusic,etoro,evite,ebay,iplayer,ph,kwh,dbase,vsphere,";
        // Abbreviations written with a full stop that does not end a sentence or title: "this v. that", "Mr. and Mrs. Smith"
        public const string ABBREVS_WITH_PERIOD = ",v,vs,mr,mrs,ms,dr,prof,st,";
        public const string WORDS_WITH_AMPERSAND =",q&a,at&t,h&m,a&w,p&o,s&p,b&o,t&t,r&d,b&b,e&oe,";
    }
}
