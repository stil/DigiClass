using System.IO;

namespace DigiClass.Methods
{
    public interface IModelSerializer<TModel>
    {
        void Serialize(TextWriter writer, TModel model);
        TModel Deserialize(TextReader reader);
    }
}