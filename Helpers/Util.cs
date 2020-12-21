using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.Entities;

namespace WebApi.Helpers
{
    public abstract class Util
    {
        public const string avatarsFolder = "Images/Avatars/";
        public const string photosFolder = "Images/Photos/";
        public static string GetHashString(byte[] hashed)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashed)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        public static IMongoCollection<T> GetCollection<T>(IMongoDatabase database, string collectionName) 
            where T : EntityBase
        {
            var collectionNames = database.ListCollectionNames().ToList();
            if (!collectionNames.Contains(collectionName)) database.CreateCollection(collectionName);
            return database.GetCollection<T>(collectionName);
        }
    }
}
