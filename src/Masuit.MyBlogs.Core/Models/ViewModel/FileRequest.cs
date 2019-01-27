using Newtonsoft.Json;
using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    public class FileRequest
    {
        [JsonProperty("action")]
        public string Action { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("item")]
        public string Item { get; set; }
        [JsonProperty("newItemPath")]
        public string NewItemPath { get; set; }
        [JsonProperty("items")]
        public List<string> Items { get; set; }
        [JsonProperty("newPath")]
        public string NewPath { get; set; }
        [JsonProperty("singleFilename")]
        public string SingleFilename { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("perms")]
        public string Perms { get; set; }
        [JsonProperty("permsCode")]
        public string PermsCode { get; set; }
        [JsonProperty("recursive")]
        public bool Recursive { get; set; }
        [JsonProperty("destination")]
        public string Destination { get; set; }
        [JsonProperty("compressedFilename")]
        public string CompressedFilename { get; set; }
        [JsonProperty("folderName")]
        public string FolderName { get; set; }
        [JsonProperty("toFilename")]
        public string ToFilename { get; set; }
    }

}