using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.spacepuppyeditor
{

    [InitializeOnLoad]
    public static class SPUndo
    {

        public struct UndoRedoInfo
        {
            public string groupName;
            public int groupId;
            public bool isRedo;
        }

        public static event System.Action<UndoRedoInfo> undoRedoEvent;

#if UNITY_2022_2
        static SPUndo()
        {
            Undo.undoRedoEvent += UndoRedoEventHandler;
        }

        static void UndoRedoEventHandler(in UnityEditor.UndoRedoInfo info)
        {
            undoRedoEvent?.Invoke(new UndoRedoInfo()
            {
                groupId = info.undoGroup,
                groupName = info.undoName,
                isRedo = info.isRedo,
            });
        }
#else
        delegate void GetRecordsDelegate(List<string> records, out int cursor);

        static GroupInfo _lastGroup;

        static List<string> _records = new List<string>();
        static int _cursorPos;
        static GetRecordsDelegate _getRecordsCallback;

        static SPUndo()
        {
            unsafe
            {
                var tp = typeof(Undo);
                var methinfo = tp.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(o => o.Name == "GetRecords").FirstOrDefault(o =>
                {
                    var parr = o.GetParameters();
                    return parr.Length == 2 && parr[0].ParameterType == typeof(List<string>) && parr[1].IsOut;
                });
                if (methinfo != null)
                {
                    _getRecordsCallback = methinfo.CreateDelegate(typeof(GetRecordsDelegate)) as GetRecordsDelegate;
                }
                if (_getRecordsCallback == null)
                {
                    Debug.Log("SPUndo - failed to locate Undo.GetRecords, this version of Unity is not supported.");
                }
            }
            if (_getRecordsCallback == null) return;

            Undo.undoRedoPerformed += UndoRedoPerformed;
            Undo.willFlushUndoRecord += WillFlushUndoRecord;
            
            //initialization in editor sometimes happens outside of the main loop and 'GetRecords' call only works on main loop
            EditorHelper.Invoke(() =>
            {
                _lastGroup = GroupInfo.GetCurrent();
            }, 0f);
        }

        static void UndoRedoPerformed()
        {
            if (_getRecordsCallback == null) return;

            var cur = GroupInfo.GetCurrent();

            UndoRedoInfo outinfo;
            if (cur.cursor > _lastGroup.cursor)
            {
                //redo
                outinfo = new UndoRedoInfo()
                {
                    groupName = cur.name,
                    groupId = cur.id,
                    isRedo = true,
                };
            }
            else
            {
                //undo
                outinfo = new UndoRedoInfo()
                {
                    groupName = _lastGroup.name,
                    groupId = _lastGroup.id,
                    isRedo = false,
                };
            }

            _lastGroup = cur;
            undoRedoEvent?.Invoke(outinfo);
        }

        static void WillFlushUndoRecord()
        {
            if (_getRecordsCallback == null) return;

            var cur = GroupInfo.GetCurrent();
            if (cur.id != _lastGroup.id)
            {
                _lastGroup = cur;
            }
        }


        struct GroupInfo
        {
            public string name;
            public int id;
            public int cursor;

            public static GroupInfo GetCurrent()
            {
                _getRecordsCallback?.Invoke(_records, out _cursorPos);
                return new GroupInfo()
                {
                    name = Undo.GetCurrentGroupName(),
                    id = Undo.GetCurrentGroup(),
                    cursor = _cursorPos,
                };
            }

        }

#endif

    }

}
