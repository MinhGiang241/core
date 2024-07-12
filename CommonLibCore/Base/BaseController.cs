using Microsoft.AspNetCore.Mvc;
#pragma warning disable IDE1006
namespace CommonLibCore.Base
{
    // Định nghĩa lớp BaseController kế thừa từ ControllerBase
    public class BaseController : ControllerBase
    {
        // Phương thức để lấy địa chỉ IP của người gửi yêu cầu hiện tại
        protected string ipAddress()
        {
            // Kiểm tra xem header có chứa "X-Forwarded-For" không (thường được sử dụng để xác định địa chỉ IP gốc khi sử dụng proxy)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                // Nếu không có, lấy địa chỉ IP từ kết nối hiện tại
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        protected CommonLibCore.Model.Response Success(object data, string message = "")
        {
            // Nếu dữ liệu là null, tạo đối tượng Response với dữ liệu null và thông điệp (nếu có)
            if (data == null)
                return new CommonLibCore.Model.Response() { Data = data, Message = message };

            // Lấy tên của kiểu dữ liệu
            string name = data.GetType().Name;

            // Nếu tên bắt đầu bằng "List", tạo đối tượng PagingModel để phản hồi với trạng thái thành công
            if (name.StartsWith("List"))
            {
                return new CommonLibCore.Model.PagingModel()
                {
                    Status = CommonLibCore.Model.Response.SUCCESS,
                    Code = CommonLibCore.Model.Response.CODE_SUCCESS,
                    Data = data,
                    Message = message
                };
            }

            // Tạo đối tượng Response với trạng thái thành công
            var response = new CommonLibCore.Model.Response()
            {
                Status = CommonLibCore.Model.Response.SUCCESS,
                Code = CommonLibCore.Model.Response.CODE_SUCCESS,
                Data = data,
                Message = message
            };

            return response;
        }
    }
}
