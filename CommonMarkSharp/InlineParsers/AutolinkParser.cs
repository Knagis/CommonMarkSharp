﻿using CommonMarkSharp.Inlines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonMarkSharp.InlineParsers
{
    public class AutolinkParser : IInlineParser<Link>
    {
        // scheme = 'coap'|'doi'|'javascript'|'aaa'|'aaas'|'about'|'acap'|'cap'|'cid'|'crid'|'data'|'dav'|
        //          'dict'|'dns'|'file'|'ftp'|'geo'|'go'|'gopher'|'h323'|'http'|'https'|'iax'|'icap'|'im'|
        //          'imap'|'info'|'ipp'|'iris'|'iris.beep'|'iris.xpc'|'iris.xpcs'|'iris.lwz'|'ldap'|'mailto'|
        //          'mid'|'msrp'|'msrps'|'mtqp'|'mupdate'|'news'|'nfs'|'ni'|'nih'|'nntp'|'opaquelocktoken'|
        //          'pop'|'pres'|'rtsp'|'service'|'session'|'shttp'|'sieve'|'sip'|'sips'|'sms'|'snmp'|
        //          'soap.beep'|'soap.beeps'|'tag'|'tel'|'telnet'|'tftp'|'thismessage'|'tn3270'|'tip'|'tv'|
        //          'urn'|'vemmi'|'ws'|'wss'|'xcon'|'xcon-userid'|'xmlrpc.beep'|'xmlrpc.beeps'|'xmpp'|'z39.50r'|
        //          'z39.50s'|'adiumxtra'|'afp'|'afs'|'aim'|'apt'|'attachment'|'aw'|'beshare'|'bitcoin'|
        //          'bolo'|'callto'|'chrome'|'chrome-extension'|'com-eventbrite-attendee'|'content'|'cvs'|
        //          'dlna-playsingle'|'dlna-playcontainer'|'dtn'|'dvb'|'ed2k'|'facetime'|'feed'|'finger'|'fish'|
        //          'gg'|'git'|'gizmoproject'|'gtalk'|'hcp'|'icon'|'ipn'|'irc'|'irc6'|'ircs'|'itms'|'jar'|'jms'|
        //          'keyparc'|'lastfm'|'ldaps'|'magnet'|'maps'|'market'|'message'|'mms'|'ms-help'|'msnim'|
        //          'mumble'|'mvn'|'notes'|'oid'|'palm'|'paparazzi'|'platform'|'proxy'|'psyc'|'query'|'res'|
        //          'resource'|'rmi'|'rsync'|'rtmp'|'secondlife'|'sftp'|'sgn'|'skype'|'smb'|'soldat'|'spotify'|
        //          'ssh'|'steam'|'svn'|'teamspeak'|'things'|'udp'|'unreal'|'ut2004'|'ventrilo'|'view-source'|
        //          'webcal'|'wtai'|'wyciwyg'|'xfire'|'xri'|'ymsgr';

        private static readonly HashSet<string> _schemes = new HashSet<string>(new[]
        {
            "coap", "doi", "javascript", "aaa", "aaas", "about", "acap", "cap", "cid",
            "crid", "data", "dav", "dict", "dns", "file", "ftp", "geo", "go", "gopher",
            "h323", "http", "https", "iax", "icap", "im", "imap", "info", "ipp", "iris",
            "iris.beep", "iris.xpc", "iris.xpcs", "iris.lwz", "ldap", "mailto", "mid",
            "msrp", "msrps", "mtqp", "mupdate", "news", "nfs", "ni", "nih", "nntp",
            "opaquelocktoken", "pop", "pres", "rtsp", "service", "session", "shttp",
            "sieve", "sip", "sips", "sms", "snmp", "soap.beep", "soap.beeps", "tag",
            "tel", "telnet", "tftp", "thismessage", "tn3270", "tip", "tv", "urn", "vemmi",
            "ws", "wss", "xcon", "xcon-userid", "xmlrpc.beep", "xmlrpc.beeps", "xmpp",
            "z39.50r", "z39.50s", "adiumxtra", "afp", "afs", "aim", "apt", "attachment",
            "aw", "beshare", "bitcoin", "bolo", "callto", "chrome", "chrome-extension",
            "com-eventbrite-attendee", "content", "cvs", "dlna-playsingle",
            "dlna-playcontainer", "dtn", "dvb", "ed2k", "facetime", "feed", "finger",
            "fish", "gg", "git", "gizmoproject", "gtalk", "hcp", "icon", "ipn", "irc",
            "irc6", "ircs", "itms", "jar", "jms", "keyparc", "lastfm", "ldaps", "magnet",
            "maps", "market", "message", "mms", "ms-help", "msnim", "mumble", "mvn", "notes",
            "oid", "palm", "paparazzi", "platform", "proxy", "psyc", "query", "res", "resource",
            "rmi", "rsync", "rtmp", "secondlife", "sftp", "sgn", "skype", "smb", "soldat",
            "spotify", "ssh", "steam", "svn", "teamspeak", "things", "udp", "unreal", "ut2004",
            "ventrilo", "view-source", "webcal", "wtai", "wyciwyg", "xfire", "xri", "ymsgr"
        }, StringComparer.OrdinalIgnoreCase);

        private static readonly CharSet _nonUriChars = Patterns.ControlChars + "<>";

        public AutolinkParser(Parsers parsers)
        {
            Parsers = parsers;
            _uriParser = new Lazy<IInlineParser<InlineString>>(() => new CompositeInlineParser<InlineString>(
                parsers.EntityParser,
                new AllExceptParser(_nonUriChars)
            ));
        }

        public Parsers Parsers { get; private set; }
        public Lazy<IInlineParser<InlineString>> _uriParser { get; private set; }

        public string StartsWithChars
        {
            get { return "<"; }
        }

        public bool CanParse(Subject subject)
        {
            return subject.Char == '<';
        }

        public Link Parse(ParserContext context, Subject subject)
        {
            if (!CanParse(subject)) return null;

            var saved = subject.Save();
            subject.Advance();
            var scheme = subject.TakeWhileNot(':');

            if (subject.Char == ':' && _schemes.Contains(scheme, StringComparer.InvariantCultureIgnoreCase))
            {
                subject.Advance();
                var inlines = new List<InlineString>(new[] { new InlineString(scheme + ":") });
                var uriInlines = _uriParser.Value.ParseMany(context, subject);
                if (subject.Char == '>' && uriInlines.Any())
                {
                    subject.Advance();
                    inlines.AddRange(uriInlines);
                    var uri = string.Join("", inlines.Select(i => i.Value));
                    return new Link(
                        new LinkLabel(uri, inlines),
                        new LinkDestination(uri),
                        new LinkTitle()
                    );
                }
            }

            saved.Restore();
            return null;
        }
    }
}
