//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;

public abstract class RebuildIndexs<R> : TableFilter<R> where R : Record, new()
{
    List<IndexFile<R>> indexes;
    public bool Run(Table<R> table, R rec, List<IndexFile<R>> indexes)
    {
        this.indexes = indexes;
        table.Filter(this, rec);
        return true;
    }
    public override void FilterInit(out int maxField)
    {
        ActiveOnly = true;
        maxField = int.MaxValue;
    }
    public override FilterResult FilterRecord(R rec)
    {
        for (int i = 0; i < indexes.Count; i++)
        {
            indexes[i].Create(rec);
        }
        return FilterResult.Continue;
    }
}