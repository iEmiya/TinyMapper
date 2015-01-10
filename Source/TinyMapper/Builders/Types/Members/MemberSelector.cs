﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinyMapper.Configs;
using TinyMapper.Nelibur.Sword.Extensions;

namespace TinyMapper.Builders.Types.Members
{
    internal sealed class MemberSelector
    {
        private readonly MapConfig _config = new MapConfig();

        internal List<MappingMember> GetMappingMembers(Type sourceType, Type targetType)
        {
            List<MemberInfo> sourceMembers = GetSourceMembers(sourceType);
            List<MemberInfo> targetMembers = GetTargetMembers(targetType);

            return GetMappingMembers(sourceMembers, targetMembers, _config.Match);
        }

        private static List<MemberInfo> GetPublicMembers(Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                       .Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field)
                       .ToList();
        }

        private List<MappingMember> GetMappingMembers(List<MemberInfo> source, List<MemberInfo> target,
            Func<string, string, bool> memberMatcher)
        {
            var result = new List<MappingMember>();

            foreach (MemberInfo targetMember in target)
            {
                MemberInfo sourceMember = source.FirstOrDefault(x => memberMatcher(x.Name, targetMember.Name));
                if (sourceMember.IsNull())
                {
                    continue;
                }
                var mappingMember = new MappingMember(sourceMember, targetMember);
                result.Add(mappingMember);
            }
            return result;
        }

        private List<MemberInfo> GetSourceMembers(Type sourceType)
        {
            var result = new List<MemberInfo>();

            List<MemberInfo> members = GetPublicMembers(sourceType);
            foreach (MemberInfo member in members)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    MethodInfo method = ((PropertyInfo)member).GetGetMethod();
                    if (method.IsNull())
                    {
                        continue;
                    }
                }
                result.Add(member);
            }
            return result;
        }

        private List<MemberInfo> GetTargetMembers(Type targetType)
        {
            var result = new List<MemberInfo>();

            List<MemberInfo> members = GetPublicMembers(targetType);
            foreach (MemberInfo member in members)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    MethodInfo method = ((PropertyInfo)member).GetSetMethod();
                    if (method.IsNull() || method.GetParameters().Length != 1)
                    {
                        continue;
                    }
                }
                result.Add(member);
            }
            return result;
        }
    }
}