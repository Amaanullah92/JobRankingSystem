using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class AVLTreeService
    {
        // AVL Tree Node
        public class AVLNode
        {
            public Candidate Data;
            public AVLNode Left;
            public AVLNode Right;
            public int Height;

            public AVLNode(Candidate data)
            {
                Data = data;
                Height = 1;
            }
        }

        private AlgorithmTrace _trace;
        private int _stepId;

        public AlgorithmTrace InsertAndGetTrace(List<Candidate> candidates)
        {
            _trace = new AlgorithmTrace { AlgorithmName = "AVL Tree Construction" };
            _stepId = 1;

            AVLNode root = null;
            foreach (var c in candidates)
            {
                _trace.Steps.Add(new AlgorithmStep
                {
                    StepId = _stepId++,
                    Description = $"Inserting Candidate {c.Id} ({c.FullName})",
                    // StateSnapshot could be a flattened tree representation or string
                    Variables = new Dictionary<string, string> { { "InsertVal", c.FullName } }
                });
                root = Insert(root, c);
            }
            return _trace;
        }

        private AVLNode Insert(AVLNode node, Candidate key)
        {
            if (node == null)
                return new AVLNode(key);

            // Sort by Id for now, or Experience
            if (key.Id < node.Data.Id)
                node.Left = Insert(node.Left, key);
            else if (key.Id > node.Data.Id)
                node.Right = Insert(node.Right, key);
            else // Duplicate keys not allowed
                return node;

            // Update height
            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

            // Get balance factor
            int balance = GetBalance(node);

            // Left Left Case
            if (balance > 1 && key.Id < node.Left.Data.Id)
            {
                _trace.Steps.Add(new AlgorithmStep { StepId = _stepId++, Description = "Right Rotation (LL Case)" });
                return RightRotate(node);
            }

            // Right Right Case
            if (balance < -1 && key.Id > node.Right.Data.Id)
            {
                _trace.Steps.Add(new AlgorithmStep { StepId = _stepId++, Description = "Left Rotation (RR Case)" });
                return LeftRotate(node);
            }

            // Left Right Case
            if (balance > 1 && key.Id > node.Left.Data.Id)
            {
                _trace.Steps.Add(new AlgorithmStep { StepId = _stepId++, Description = "Left-Right Rotation (LR Case)" });
                node.Left = LeftRotate(node.Left);
                return RightRotate(node);
            }

            // Right Left Case
            if (balance < -1 && key.Id < node.Right.Data.Id)
            {
                _trace.Steps.Add(new AlgorithmStep { StepId = _stepId++, Description = "Right-Left Rotation (RL Case)" });
                node.Right = RightRotate(node.Right);
                return LeftRotate(node);
            }

            return node;
        }

        private int GetHeight(AVLNode node) => node == null ? 0 : node.Height;

        private int GetBalance(AVLNode node) => node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);

        private AVLNode RightRotate(AVLNode y)
        {
            AVLNode x = y.Left;
            AVLNode T2 = x.Right;

            // Perform rotation
            x.Right = y;
            y.Left = T2;

            // Update heights
            y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;
            x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;

            return x;
        }

        private AVLNode LeftRotate(AVLNode x)
        {
            AVLNode y = x.Right;
            AVLNode T2 = y.Left;

            // Perform rotation
            y.Left = x;
            x.Right = T2;

            // Update heights
            x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;
            y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;

            return y;
        }
    }
}
