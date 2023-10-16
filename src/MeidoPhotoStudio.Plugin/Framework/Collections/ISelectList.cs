using System.Collections.Generic;

namespace MeidoPhotoStudio.Plugin.Framework.Collections;

public interface ISelectList<T> : IList<T>
{
    int CurrentIndex { get; set; }

    T Current { get; }

    T SetCurrentIndex(int index);

    T Next();

    T Previous();

    T CycleNext();

    T CyclePrevious();
}
