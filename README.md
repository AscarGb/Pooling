## ObjectPool

### Example
```c#
List<long> Creator() => new List<long>(1024 * 1024);

void Clearer(List<long> l) => l.ForEach(i => i = 0);

var bigListPool = new Pool<List<long>>(Creator, Clearer, 10);

using (var pooledList = bigListPool.Rent())
{
  var list = pooledList.item;
  //work with object
  
}//return to pool
```
  
