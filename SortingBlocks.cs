using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTGAlphaSortDivider
{
    public enum TopLevelSortingBlocks
    {
        White,
        Blue,
        Black,
        Red,
        Green,
        Multicolor,
        Colorless,
        Lands
    }

    public enum SecondLevelBlocks
    {
        CreaturesArtifacts,
        InstantsSorceries,
        Enchantments
    }
}
