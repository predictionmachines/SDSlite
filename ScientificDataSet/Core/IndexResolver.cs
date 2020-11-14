// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
    internal sealed class IndexResolver
    {
        private LinkedList<int[]> currentSet;
        private Dictionary<string, int> dimIndexes;

        public IndexResolver(string[] dims)
        {
            currentSet = new LinkedList<int[]>();
            dimIndexes = new Dictionary<string, int>(dims.Length);

            for (int i = 0; i < dims.Length; i++)
            {
                dimIndexes[dims[i]] = i;
            }
        }

        public int[][] GetResolvedIndices()
        {
            return currentSet.ToArray();
        }

        public void Resolve(int[][] indexSet, string[] dims)
        {
            if (currentSet.Count == 0)
            {
                for (int i = 0; i < indexSet.Length; i++)
                {
                    int[] set = new int[dimIndexes.Count];
                    for (int j = 0; j < dimIndexes.Count; j++)
                        set[j] = -1;
                    for (int j = 0; j < dims.Length; j++)
                    {
                        set[dimIndexes[dims[j]]] = indexSet[i][j];
                    }
                    currentSet.AddLast(set);
                }

                return;
            }


            LinkedListNode<int[]> current = currentSet.First;

            while(current != null)
            {
                for (int i = 0; i < indexSet.Length; i++)
                {
                    int[] resultRes = IndexSetEquals(current.Value, indexSet[i], dims);
                    if (resultRes != null)
                    {
                        currentSet.AddBefore(current, resultRes);
                    }
                }
                LinkedListNode<int[]> toRemove = current;
                current = current.Next;
                currentSet.Remove(toRemove);
            }
        }

        private int[] IndexSetEquals(int[] nativeSet, int[] alienSet, string[] dims)
        {
            // Intersection of two sets will be here:
            int[] resultSet = new int[nativeSet.Length];
            for (int j = 0; j < resultSet.Length; j++)
                resultSet[j] = -1;

            for (int i = 0; i < alienSet.Length; i++)
            {
                int j = dimIndexes[dims[i]];

                if (nativeSet[j] == -1 || nativeSet[j] == alienSet[i])
                    resultSet[j] = alienSet[i];
                else
                    return null;
            }

            return resultSet;
        }
    }
}

