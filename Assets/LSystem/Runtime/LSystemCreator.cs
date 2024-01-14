using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace Sayama.LSystem
{
	
	/// <summary>
	/// Struct representing parameters for string generation using an L-system. 
	/// An L-system is a rewriting system and a type of formal grammar. 
	/// It consists of an alphabet of symbols that can be used to make strings,
	/// a collection of production rules that expand each symbol into some larger string of symbols,
	/// an initial string from which to begin construction, and a mechanism for translating the generated strings into geometric structures. 
	/// </summary>
	public struct LSystemStringParams
	{
		/// <summary>
		/// The initial string to start the L-system process. 
		/// This string is iteratively transformed by the rules defined in the Rules property.
		/// </summary>
		public string InitialString;

		/// <summary>
		/// A dictionary of char to string mappings to replace each character in InitialString 
		/// during each iteration. The char is the character to be replaced, and the string is what it gets replaced with.
		/// </summary>
		public Dictionary<char, string> Rules;

		/// <summary>
		/// The number of times the rules are applied to transform the InitialString.
		/// </summary>
		public int IterationCount;

		/// <summary>
		/// The degree in which some graphical representations of the generated string 
		/// may be rotated after each iteration, not used in this current context. 
		/// </summary>
		public float RotationDegrees;

		public float GenerateRandomDistance()
		{
			return Random.Range(2.0f, 5.0f);
		}

		public Quaternion GenerateRandomRotation(Quaternion rotation)
		{
			return Quaternion.Euler(0, GenerateRandomRotation(),GenerateRandomRotation()) * rotation * Quaternion.Euler(GenerateRandomRotation(),0,0);
            // return GenerateQuaternion(rotation, direction,GenerateRandomRotation());
		}

		public Quaternion GenerateRandomRightRotation(Quaternion rotation)
		{
			return Quaternion.Euler(0, GenerateRandomRightRotation(),GenerateRandomRightRotation()) * rotation * Quaternion.Euler(GenerateRandomRightRotation(),0,0);
            // return GenerateQuaternion(rotation, direction,GenerateRandomRightRotation());
		}

		public Quaternion GenerateRandomLeftRotation(Quaternion rotation)
		{
			return Quaternion.Euler(0, GenerateRandomLeftRotation(),GenerateRandomLeftRotation()) * rotation * Quaternion.Euler(GenerateRandomLeftRotation(),0,0);
			// return GenerateQuaternion(rotation, direction,GenerateRandomLeftRotation());
		}
		public float GenerateRandomRotation()
		{
			return Random.Range(-RotationDegrees*0.05f, +RotationDegrees*0.05f) + RotationDegrees * (Random.value > .5f ? 1 : -1);
		}
		public float GenerateRandomRightRotation()
		{
			return Random.Range(-RotationDegrees*0.05f, +RotationDegrees*0.05f) + RotationDegrees;
		}
		public float GenerateRandomLeftRotation()
		{
			return Random.Range(-RotationDegrees*0.05f, +RotationDegrees*0.05f) + RotationDegrees * -1;
		}

		private Quaternion GenerateQuaternion(Quaternion rotation, Vector3 direction, float degrees)
		{
			float fullCircleRandom = Random.Range(-180.0f, +180.0f);
			var rot = Quaternion.AngleAxis(fullCircleRandom, direction);
			var inverse = Quaternion.Inverse(rotation);
			return Quaternion.SlerpUnclamped(rotation, inverse, degrees / 180.0f) * rot;
		}
		
		public static LSystemStringParams CreateDefault()
		{
			return new()
			{
				Rules = new()
				{
					{ 'F', " F+F−F−F+F" }
				},
				IterationCount = 1,
				InitialString = "F",
				RotationDegrees = 22.5f,
			};
		}
	}

	public struct LSystemNode
	{
		public Vector3 Position;
		public Quaternion Rotation;
		public Transform Object;
		public Vector3 GetDirection()
		{
			return Rotation * Vector3.up;
		}
	}

	public static class LSystemCreator
	{
		/// <summary>
		/// Generates a string based on the provided LSystemStringParams.
		/// </summary>
		/// <param name="parameters"> 
		/// An LSystemStringParams object which includes:
		/// - InitialString: The initial string to start the process. It must not be null or whitespace.
		/// - Rules: A List of key/value pairs which govern how each character in InitialString 
		///   is replaced in each iteration. It must not be null and must contain at least one rule.
		/// - IterationCount: The number of times the rules are applied. It must not be a negative number.
		/// The method iterates through each character of the InitialString for the count of IterationCount, 
		/// replacing it according to the rules.
		/// </param>
		/// <returns>A new string which is the result of applying the rules to InitialString for the number of times specified by IterationCount.</returns>
		/// <exception cref="AssertionException">Throws an exception if InitialString is null/white space, 
		/// if Rules is null/empty, or if IterationCount is less than zero.</exception>
		public static string GenerateString(LSystemStringParams parameters)
		{
			Assert.IsNotNull(parameters.InitialString.Trim(), "parameters.InitialString.Trim()");
			Assert.IsNotNull(parameters.Rules, "parameters.Rules");
			Assert.IsTrue(parameters.Rules.Count > 0, "parameters.Rules.Count > 0");
			Assert.IsTrue(parameters.IterationCount >= 0, "parameters.IterationCount >= 0");

			string result = parameters.InitialString.Trim();

			for (int i = 0; i < parameters.IterationCount; i++)
			{
				int stringSize = result.Length;
				for (int j = 0; j < stringSize; j++)
				{
					foreach (var rule in parameters.Rules)
					{
						if (result[j] == rule.Key)
						{
							int valueSize = rule.Value.Length;
							result = result.Remove(j, 1);
							result = result.Insert(j, rule.Value);
							stringSize += (valueSize - 1);
							j += (valueSize - 1);
						}
					}
				}
			}

			return result;
		}

		public static void GenerateLSystem(Transform root, LSystemStringParams parameters)
		{
			string lsystem = GenerateString(parameters);

			Stack<LSystemNode> nodeStack = new ();
			nodeStack.Push(new LSystemNode
			{
				Position = Vector3.zero,
				Rotation = Quaternion.identity,
				Object = root,
			});

			for (int i = 0; i < lsystem.Length; i++)
			{
				char key = lsystem[i];
				switch (key)
				{
					case 'F':
					{
						var node = nodeStack.Peek();
						var distance = parameters.GenerateRandomDistance();
						node.Position += node.GetDirection() * distance;
						
						// Create newNodeObject
						var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						obj.name =$"Node_{i}";
						var trs = obj.transform;
						trs.SetParent(node.Object, false);
						// trs.rotation = node.Rotation;
						trs.position = node.Position;
						node.Object = trs;
						nodeStack.Replace(node);
					}
                    	break;
					case '+':
					{
						var node = nodeStack.Peek();
                        node.Rotation = parameters.GenerateRandomRightRotation(node.Rotation);
                        nodeStack.Replace(node);
					}
                    	break;
					case '-':
                    {
						var node = nodeStack.Peek();
                        node.Rotation = parameters.GenerateRandomLeftRotation(node.Rotation);
                        nodeStack.Replace(node);
					}
                    	break;
					case '[':
                    {
						var node = nodeStack.Peek();
                        node.Rotation = parameters.GenerateRandomRotation(node.Rotation);
						nodeStack.Push(node);
					}
                    	break;
					case ']':
                    {
						var node = nodeStack.Pop();
					}
                    	break;
					default:
						Debug.LogError($"The character '{key}'({(int)key}) is unknown.");
						Debug.LogError($"The character '{'-'}'({(int)'-'}) is unknown.");
						break;
				}
			}

			CreateRenderer(root);
		}

		private static void CreateRenderer(Transform obj)
		{
			Vector3 position = obj.position;
			List<GameObject> objects = new List<GameObject>();
			for (int i = 0; i < obj.childCount; i++)
			{
				var child = obj.GetChild(i);
				
				GameObject renderer = GameObject.CreatePrimitive(PrimitiveType.Cube);
				objects.Add(renderer);
				renderer.name = $"{obj.name}_to_{child.name}";
	
				var childPos = child.position;
				var direction = childPos - position;

				var trs = renderer.transform;
				trs.position = position + (direction * 0.5f);
				var forward = direction.normalized;
				trs.rotation = Quaternion.LookRotation(forward);
				trs.localScale = new Vector3(.65f, .65f, direction.magnitude);
	
				CreateRenderer(child);
			}

			foreach (var gameObject in objects)
			{
				gameObject.transform.SetParent(obj, true);
			}
		}
	}
}