// MIT License

// DeusaldUnityTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeusaldUnityTools
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class DeusaldInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            MonoBehaviour mono = (MonoBehaviour)target;
            Type type = mono.GetType();

            DrawEditorButtons(mono, type);
            DrawShowInInspector(mono, type);
        }

        private void DrawEditorButtons(MonoBehaviour mono, Type type)
        {
            IEnumerable<MethodInfo> methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static |
                            BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => Attribute.IsDefined(m, typeof(ButtonAttribute)));

            foreach (MethodInfo method in methods)
            {
                if (GUILayout.Button(method.Name))
                {
                    method.Invoke(mono, null);
                }
            }
        }

        private void DrawShowInInspector(MonoBehaviour mono, Type type)
        {
            IEnumerable<MemberInfo> members = type
                .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => Attribute.IsDefined(m, typeof(ShowInInspectorAttribute)));

            foreach (MemberInfo member in members)
            {
                ShowInInspectorAttribute attr = member.GetCustomAttribute<ShowInInspectorAttribute>();
                bool readOnly = attr?.ReadOnly ?? false;

                EditorGUI.BeginDisabledGroup(readOnly);

                if (member is FieldInfo field)
                {
                    DrawValue(
                        ObjectNames.NicifyVariableName(field.Name),
                        field.FieldType,
                        field.GetValue(mono),
                        v => field.SetValue(mono, v)
                    );
                }
                else if (member is PropertyInfo prop && prop.CanRead)
                {
                    DrawValue(
                        ObjectNames.NicifyVariableName(prop.Name),
                        prop.PropertyType,
                        prop.GetValue(mono),
                        v =>
                        {
                            if (prop.CanWrite)
                                prop.SetValue(mono, v);
                        }
                    );
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawValue(string label, Type type, object value, Action<object> setValue)
        {
            EditorGUI.BeginChangeCheck();

            object newValue = value;

            if (type == typeof(int))
                newValue = EditorGUILayout.IntField(label, (int)value);

            else if (type == typeof(float))
                newValue = EditorGUILayout.FloatField(label, (float)value);

            else if (type == typeof(bool))
                newValue = EditorGUILayout.Toggle(label, (bool)value);

            else if (type == typeof(string))
                newValue = EditorGUILayout.TextField(label, (string)value);

            else if (type.IsEnum)
                newValue = EditorGUILayout.EnumPopup(label, (Enum)value);

            else if (type == typeof(Vector2))
                newValue = EditorGUILayout.Vector2Field(label, (Vector2)value);

            else if (type == typeof(Vector3))
                newValue = EditorGUILayout.Vector3Field(label, (Vector3)value);

            else
                EditorGUILayout.LabelField(label, $"({type.Name})");

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, $"Modify {label}");
                setValue?.Invoke(newValue);
                EditorUtility.SetDirty(target);
            }
        }
    }
}