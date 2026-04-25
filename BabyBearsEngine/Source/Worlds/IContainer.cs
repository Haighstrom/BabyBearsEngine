using System.Collections.Generic;
using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Worlds;

public interface IContainer
{
    void Add(IAddable entity);
    void Remove(IAddable entity);
    void RemoveAll();
    (float x, float y) GetWindowCoordinates(float x, float y);
}
