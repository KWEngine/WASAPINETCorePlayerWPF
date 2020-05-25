using System;
using System.Collections.Generic;
using System.Text;

namespace WASAPINETCore.Audio
{
    class FileDetails
    {
        public string Artist { get; set; } = "(not available)";
        public string Title { get; set; } = "(not available)";
        public string Album { get; set; } = "(not available)";

        public bool StreamOK { get; set; } = false;

        public FileDetails()
            :this("", "", "")
        {

        }
        public FileDetails(string ar, string t, string al)
        {
            Artist = ar == null || ar.Trim().Length < 1 ? "(not available)" : ar.Trim();
            Title = t == null || t.Trim().Length < 1 ? "(not available)" : t.Trim();
            Album = al == null || al.Trim().Length < 1 ? "(not available)" : al.Trim();
        }
    }
}
