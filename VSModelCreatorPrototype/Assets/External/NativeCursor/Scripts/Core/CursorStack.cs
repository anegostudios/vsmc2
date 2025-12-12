using System.Collections.Generic;
using UnityEngine;

namespace Riten.Native.Cursors
{
    public struct CursorStackItem
    {
        public NTCursors cursor;
        public int id;
        public int priority;
        public int secondaryPriority;
    }
    
    public static class CursorStack
    {
        static int _nextUid = 1;
        static bool _paused;
        
        static readonly List<CursorStackItem> _stack = new ();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            _stack.Clear();
            _nextUid = 1;
            _paused = false;
        }
        
        static void OnStackChanged(bool force = false)
        {
            if (!force && _paused) return;
            
            if (_stack.Count == 0)
                 NativeCursor.ResetCursor();
            else NativeCursor.SetCursor(Peek().cursor);
        }

        /// <summary>
        /// A debug GUI to see the current stack.
        /// Uses GUILayout, so it's not very customizable.
        /// Just dumps the stack from top to bottom.
        /// </summary>
        public static void OnDebugGUI()
        {
            GUILayout.Label($"Stack count: {_stack.Count}");
            GUILayout.Label($"Paused: {_paused}");
            
            var peek = Peek();
            
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (peek.id == _stack[i].id)
                    GUI.color = Color.green;
                GUILayout.Label($"{_stack[i].id}: Cursor {_stack[i].cursor} with priority {_stack[i].priority} and secondary priority {_stack[i].secondaryPriority}");
                GUI.color = Color.white;
            }
        }
        
        /// <summary>
        /// If true, the cursor will not change until this is set to false.
        /// The stack will still be updated, but won't affect the cursor.
        /// Once unpaused, the cursor will be updated automatically.
        /// </summary>
        public static void PauseRendering(bool isPaused)
        {
            _paused = isPaused;
            OnStackChanged();
        }
        
        /// <summary>
        /// Call this if you used NativeCursor.SetCursor() directly and want to reapply the stack's cursor.
        /// </summary>
        public static void ReApply()
        {
            OnStackChanged(true);
        }

        /// <summary>
        /// Pushes a cursor to the stack.
        /// Higher priority means this cursor will override other cursors with lower priority.
        /// </summary>
        /// <returns>The id of the pushed cursor. Use this to remove the cursor later.</returns>
        public static int Push(NTCursors cursor, int priority = 0, int secondaryPriority = 0)
        {
            var uid = _nextUid++;
            
            _stack.Add(new CursorStackItem
            {
                cursor = cursor,
                id = uid,
                priority = priority,
                secondaryPriority = secondaryPriority
            });
            
            if (Peek().id == uid)
                OnStackChanged();

            return uid;
        }
        
        /// <summary>
        /// Pops the cursor that is being rendered.
        /// Always prefer using Pop(int id) if you have the id.
        /// </summary>
        /// <returns>True if a cursor was removed, false otherwise.</returns>
        public static bool Pop()
        {
            if (_stack.Count > 0)
            {
                var top = Peek();
                return Pop(top.id);
            }

            return false;
        }
        
        /// <summary>
        /// Pops the cursor with the given id.
        /// You can get the id from the return of Push().
        /// </summary>
        /// <returns></returns>
        public static bool Pop(int id)
        {
            if (id == 0) return false;
            
            bool removed = false;
            
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i].id == id)
                {
                    _stack.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            if (removed)
            {
                OnStackChanged();
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Removes all cursors from the stack.
        /// Resets the cursor to default.
        /// </summary>
        public static void Clear()
        {
            if (_stack.Count == 0)
                return;
            
            _stack.Clear();
            OnStackChanged();
        }
        
        /// <summary>
        /// Returns true if the stack is empty.
        /// </summary>
        public static bool IsEmpty => _stack.Count == 0;
        
        /// <summary>
        /// Returns the cursor at the top of the stack.
        /// It will be the cursor with the highest priority.
        /// </summary>
        /// <returns>Default if the stack is empty. Otherwise, the cursor with highest priority.</returns>
        public static CursorStackItem Peek()
        {
            if (_stack.Count == 0)
                return default;
            
            int highestPriority = _stack[_stack.Count - 1].priority;
            int highestSecondaryPriority = _stack[_stack.Count - 1].secondaryPriority;
            int idx = _stack.Count - 1;
                
            for (int i = _stack.Count - 2; i >= 0; i--)
            {
                if (_stack[i].priority >= highestPriority)
                {
                    if (highestPriority != _stack[i].priority)
                    {
                        highestPriority = _stack[i].priority;
                        highestSecondaryPriority = _stack[i].secondaryPriority;
                        idx = i;
                        continue;
                    }
                    
                    if (_stack[i].secondaryPriority > highestSecondaryPriority)
                    {
                        highestSecondaryPriority = _stack[i].secondaryPriority;
                        idx = i;
                    }
                }
            }
            
            return _stack[idx];
        }
        
        /// <summary>
        /// Replace the cursor with the given id with the given cursor.
        /// This will not change the priority of the cursor.
        /// </summary>
        public static bool Replace(int id, NTCursors cursor)
        {
            for(int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i].id == id)
                {
                    var oldPriority = _stack[i].priority;
                    var oldSecondaryPriority = _stack[i].secondaryPriority;
                    
                    _stack[i] = new CursorStackItem
                    {
                        cursor = cursor,
                        id = id,
                        priority = oldPriority,
                        secondaryPriority = oldSecondaryPriority
                    };
                    
                    var peek = Peek();
                    
                    if (peek.id == id)
                        OnStackChanged();
                    return true;
                }
            }

            return false;
        }
    }
}