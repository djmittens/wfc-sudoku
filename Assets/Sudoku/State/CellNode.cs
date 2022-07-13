using System.Collections.Generic;

class CellNode
{
    public int id = 0;

    public IList<CellNode> neighbors;

    public Result Visit(int pos, HashSet<int>[] spos)
    {
        if (spos[id].Contains(pos))
        {
            spos[id].Remove(pos);
            var entropy = spos[id].Count;

            switch (entropy)
            {
                case 0:
                    return Result.LowEntropy;
                case 1:
                    return Result.Collapsed;
            }
        }

        return Result.Undecided;
    }

    public enum Result { Undecided, Collapsed, LowEntropy }
}