using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Result
{
    public Object Data;
    /// <summary>
    /// 0为正常，1为系统繁忙,2为接口错误,3为配置未初始化,4为配置不正确
    /// </summary>
    public int State=0;
    public string Msg;
}
