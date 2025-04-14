using TP.ConcurrentProgramming.Presentation.Model;

namespace TP.ConcurrentProgramming.PresentationModelTest
{
  [TestClass]
  public class ModelAbstractAPITest
  {
    [TestMethod]
    public void SingletonConstructorTestMethod()
    {
      ModelAbstractApi instance1 = ModelAbstractApi.CreateModel();
      ModelAbstractApi instance2 = ModelAbstractApi.CreateModel();
      Assert.AreSame<ModelAbstractApi>(instance1, instance2);
    }
  }
}