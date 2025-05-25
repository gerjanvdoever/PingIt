using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.Services
{
    public interface IUserStore
    {
        UserDto? CurrentUser { get; set; }
    }

    public class UserStore : IUserStore
    {
        public UserDto? CurrentUser { get; set; }
    }
}
