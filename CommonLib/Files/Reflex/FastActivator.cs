using System.Linq.Expressions;
using System.Reflection;

namespace CommonLibCore.CommonLib.Reflex
{
    static class FastActivator
    {
        private static Dictionary<Type, Func<object[], object>> factoryCache =
            new Dictionary<Type, Func<object[], object>>();

        /// <summary>
        /// Creates an instance of the specified type using a generated factory to avoid using Reflection.
        /// </summary>
        /// <param Name="type">The type to be created.</param>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns>The newly created instance.</returns>
        public static object Create(Type type, params object[] args)
        {
            Func<object[], object> f;

            // Kiểm tra xem đã có hàm tạo (delegate) cho kiểu type trong factoryCache chưa
            if (!factoryCache.TryGetValue(type, out f))
                // Nếu chưa có, thực hiện lock để bảo vệ quá trình tạo hàm tạo
                lock (factoryCache)
                    // Kiểm tra lại trong lock, có thể có một luồng khác đã thêm vào factoryCache trong khi luồng hiện tại đợi lock
                    if (!factoryCache.TryGetValue(type, out f))
                    {
                        // Nếu vẫn chưa có, tạo một mảng các kiểu dữ liệu từ các đối số truyền vào
                        Type[] typeArray = args.Select(obj => obj.GetType()).ToArray();
                        // Xây dựng hàm tạo đối tượng và lưu vào factoryCache
                        factoryCache[type] = f = BuildDeletgateObj(type, typeArray);
                    }
            // Trả về đối tượng được tạo bằng cách gọi hàm tạo từ delegate f với các đối số args
            return f(args);
        }

        private static Func<object[], object> BuildDeletgateObj(Type type, Type[] typeList)
        {
            //Lấy thông tin Constructor của Type:
            ConstructorInfo constructor = type.GetConstructor(typeList);
            //Tạo biểu thức tham số (ParameterExpression):
            ParameterExpression paramExp = Expression.Parameter(typeof(object[]), "args_");
            /* Tạo danh sách biểu thức (Expression): */
            Expression[] expList = GetExpressionArray(typeList, paramExp);
            /* Tạo biểu thức New (NewExpression): */
            NewExpression newExp = Expression.New(constructor, expList);
            /* Tạo biểu thức Lambda (Expression<Func<object[], object>>): */
            Expression<Func<object[], object>> expObj = Expression.Lambda<Func<object[], object>>(
                newExp,
                paramExp
            );
            /* Compile biểu thức Lambda thành delegate: */
            return expObj.Compile();
        }

        private static Expression[] GetExpressionArray(
            Type[] typeList,
            ParameterExpression paramExp
        )
        {
            List<Expression> expList = new List<Expression>();
            for (int i = 0; i < typeList.Length; i++)
            {
                var paramObj = Expression.ArrayIndex(paramExp, Expression.Constant(i));
                var expObj = Expression.Convert(paramObj, typeList[i]);
                expList.Add(expObj);
            }

            return expList.ToArray();
        }
    }
}
