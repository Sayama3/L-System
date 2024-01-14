using System.Collections.Generic;

namespace Sayama.LSystem
{
	public static class StackExtensions
	{
		public static void Replace<T>(this Stack<T> stack, T item)
		{
			stack.Pop();
			stack.Push(item);
		}
	}
}