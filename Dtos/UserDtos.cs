using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;

namespace WebApi.Dtos
{
    public class HostDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserStatus Status { get; set; }
    }
    public class SearchUserDto
    {
        [AllowNull]
        public string Keyword = "";
        public int Take = 100;
        public int Skip = 0;
        public bool ShowRejected = true;
    }

    public class SearchResultUserDto
    {
        public List<HostDto> Result;
        public int Count;
        public int StartIndex;
        public int Total;
    }
}
