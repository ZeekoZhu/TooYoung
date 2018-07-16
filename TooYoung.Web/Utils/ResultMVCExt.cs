using System;
using Microsoft.AspNetCore.Mvc;

namespace TooYoung.Web.Utils
{

    public struct ErrorMsg
    {
        public ErrorMsg(string error)
        {
            Error = error;
        }

        public string Error { get; set; }
    }
}
