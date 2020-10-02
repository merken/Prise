using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Example.Contract
{
    public class MyDto
    {
        public int Number { get; set; }
        public string Text { get; set; }
    }

    public interface IPlugin
    {
        Task<MyDto> Create(int number, string text);
        Task<IEnumerable<MyDto>> GetAll();
    }
}
