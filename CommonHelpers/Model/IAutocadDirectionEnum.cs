using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Models
{
    public interface IAutocadDirectionEnum
    {
        public enum Direction
        {
            Right = 1,
            Above = 2,
            Left = 4,
            Below = 8
        }
    }
}
