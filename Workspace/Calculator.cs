using System;
using System.Collections;
using System.Collections.Generic;

public class Calculator
{
    private string[] _operators = { "-", "+", "/", "*","^"};
    private  Func<double, double, double>[] _operations = {
        (a1, a2) => a1 - a2,
        (a1, a2) => a1 + a2,
        (a1, a2) => a1 / a2,
        (a1, a2) => a1 * a2,
        (a1, a2) => Math.Pow(a1, a2)
    };
    private string numbers="0123456789";

    public Dictionary<string, double> StepParameters(string expression, Dictionary<string, double> parameters){
        string[] equations = expression.Split(";");
        Dictionary<string, double> parameters_to_change = new Dictionary<string, double>();

        for(int i=0;i<equations.Length;i++){
            List<string> tokens = getTokens(equations[i]);

            if(tokens.IndexOf("=")==1){
                string lpn;  //left parameter name

                //Pre-processing
                lpn = tokens[0];
                tokens.RemoveAt(0);
                tokens.RemoveAt(0);

                //parameters[lpn] = Eval(tokens, parameters);

                parameters_to_change.Add(lpn, Eval(tokens, parameters));

            }else throw new ArgumentException("Math expression is not proper");
        }

        foreach(var key in parameters_to_change.Keys){
            parameters[key] = parameters_to_change[key];
        }

        return parameters;
    }

    public double Eval(List<string> tokens, Dictionary<string, double> parameters)
    {
        //List<string> tokens = getTokens(expression);
        Stack<double> operandStack = new Stack<double>();
        Stack<string> operatorStack = new Stack<string>();
        int tokenIndex = 0;

        while (tokenIndex < tokens.Count) {
            string token = tokens[tokenIndex];
            if (token == "(") {
                List<string> subExpr = getSubExpression(tokens, ref tokenIndex);
                operandStack.Push(Eval(subExpr, parameters));   //recursion
                continue;
            }

            if (token == ")") {
                throw new ArgumentException("Mis-matched parentheses in expression");
            }

            //If this is an operator  
            if (Array.IndexOf(_operators, token) >= 0) {
                while (operatorStack.Count > 0 ){ //&& Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek())) {
                    string op = operatorStack.Pop();
                    double arg2 = operandStack.Pop();
                    double arg1 = operandStack.Pop();
                    operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                }
                operatorStack.Push(token);
            } else {
                if(numbers.IndexOf(token[0])>=0) operandStack.Push(double.Parse(token));
                else if(parameters.ContainsKey(token)) operandStack.Push(parameters[token]);
                else throw new ArgumentException("The name of parameter wasn't found");
            }
            tokenIndex += 1;
        }

        while (operatorStack.Count > 0) {
            string op = operatorStack.Pop();
            double arg2 = operandStack.Pop();
            double arg1 = operandStack.Pop();
            operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
        }

        return operandStack.Pop();
    }

    private List<string> getSubExpression(List<string> tokens, ref int index)
    {
        List<string> subExpr = new List<string>();
        int parenlevels = 1;
        index += 1;
        while (index < tokens.Count && parenlevels > 0) {
            string token = tokens[index];
            if (tokens[index] == "(") {
                parenlevels += 1;
            }

            if (tokens[index] == ")") {
                parenlevels -= 1;
            }

            if (parenlevels > 0) {
                subExpr.Add(token);
            }

            index += 1;
        }

        if ((parenlevels > 0)) {
            throw new ArgumentException("Mis-matched parentheses in expression");
        }

        return subExpr;
    }

    private List<string> getTokens(string expression)
    {
        string operators = "()^*/+-=";
        List<string> tokens = new List<string>();
        string sb = "";

        foreach (char c in expression.Replace(" ", string.Empty)) {
            if (operators.IndexOf(c) >= 0) {
                if ((sb.Length > 0)) {
                    tokens.Add(sb);
                    sb = "";
                }
                tokens.Add(c.ToString());
            } else {
                sb += c.ToString();
            }
        }

        if ((sb.Length > 0)) {
            tokens.Add(sb);
        }

        return tokens;
    }
}


// ^を先に計算する条件分岐をつけるべき
// パラメータの式に変える必要