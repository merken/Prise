using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TableStoragePlugin
{
    public class ProductTableEntity : TableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string Description { get; set; }
        public decimal PriceExlVAT { get; set; }
    }
}
