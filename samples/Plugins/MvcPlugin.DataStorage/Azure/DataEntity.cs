using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MvcPlugin.DataStorage.Azure
{
    public class DataEntity : TableEntity
    {
        public int Number { get; set; }
        public string Text { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            this.Number = properties["number"].Int32Value.Value;
            this.Text = properties["text"].StringValue;
        }
    }
}