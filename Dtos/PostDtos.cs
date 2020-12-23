using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;

namespace WebApi.Dtos
{
    public class CreateOrUpdatePostDto
    {
        [AllowNull]
        public string Id;
        public string Caption;
        public string WardCode;
        public string Address;
        public long Rent;
        public double Area;
        public string Description;
    }

    public class GetPostDto
    {
        public string Id;
        public string Caption;
        public HostDto Host;
        public PostStatus Status;
        public WardDto Ward;
        public string Address;
        public long Rent;
        public int Views;
        public double Rating;
        public List<string> Photos;
        public double Area;
        public string Description;
    }

    public class SearchPostDto
    {
        [AllowNull]
        public string CaptionKeyword = "";
        [AllowNull]
        public string AddressKeyWord = "";
        [AllowNull]
        public string WardCode;
        [AllowNull]
        public string DistrictCode;
        [AllowNull]
        public string ProvinceCode;
        public int Take = 100;
        public int Skip = 0; 
        public SortField Sort = SortField.None;
        public bool Desc = false;
        [AllowNull]
        public long? MinRent = 0;
        [AllowNull]
        public long? MaxRent = long.MaxValue;
        public bool ShowRejected = true;
    }

    public class SearchResultPostDto
    {
        public List<GetPostDto> Result;
        public int Count;
        public int StartIndex;
        public int Total;
    }

    public enum SortField
    {
        None,
        Rent,
        Rating,
        Views
    }
}
