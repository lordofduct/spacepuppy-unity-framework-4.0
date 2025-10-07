
using System;
using System.Runtime.Serialization;

using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Serialization
{

    [System.Serializable()]
    internal class PersistantUnityObjectSerializedProxy : IDeserializationCallback
    {
        
        internal void OnSerialize(IPersistantAsset obj, SerializationInfo info, StreamingContext context)
        {
            if (obj == null) return;

            info.AddValue("sp*id", obj.AssetId);
            obj.OnSerialize(info, context);
        }
        


        public void OnDeserialize(SerializationInfo info, StreamingContext context, IAssetSet assets)
        {
            _info = info;
            _context = context;
            _assets = assets;
        }

        
        private SerializationInfo _info;
        private StreamingContext _context;
        private IAssetSet _assets;

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (_assets == null) return;

            var resourceId = _info.GetString("sp*id");
            var obj = _assets.LoadAsset(resourceId);
            if (obj == null) return;

            obj = UnityEngine.Object.Instantiate(obj);

            foreach (var pobj in ObjUtil.GetAllFromSource<IPersistantAsset>(obj))
            {
                pobj.OnDeserialize(_info, _context, _assets);
            }
        }
    }
}
