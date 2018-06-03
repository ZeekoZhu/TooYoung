using System;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace TooYoung.Web.Utils
{
    public static class ResultMVCExt
    {
        public static ActionResult<T> ToActionResult<T>(this Result<T> result, Func<object, ActionResult> errorResult)
        {
            return result.IsSuccess ? (ActionResult<T>)result.Value : errorResult(new ErrorMsg(result.Error));
        }
    }

    public struct ErrorMsg
    {
        public ErrorMsg(string error)
        {
            Error = error;
        }

        public string Error { get; set; }
    }
}
