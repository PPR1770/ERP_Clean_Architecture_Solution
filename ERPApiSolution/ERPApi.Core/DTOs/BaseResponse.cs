using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPApi.Core.DTOs
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public BaseResponse()
        {
            Success = true;
        }

        public BaseResponse(T data, string message = "")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        public BaseResponse(string error)
        {
            Success = false;
            Message = error;
            Errors.Add(error);
        }

        public BaseResponse(List<string> errors)
        {
            Success = false;
            Message = errors.FirstOrDefault() ?? "An error occurred";
            Errors = errors;
        }
    }
}
