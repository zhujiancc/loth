/*
 * @Author: your name
 * @Date: 2021-12-02 15:31:13
 * @LastEditTime: 2021-12-02 15:40:58
 * @LastEditors: Please set LastEditors
 * @Description: 打开koroFileHeader查看配置 进行设置: https://github.com/OBKoro1/koro1FileHeader/wiki/%E9%85%8D%E7%BD%AE
 * @FilePath: \loth.k6\app.js
 */
import http from 'k6/http';


export default function () {
  http.get('https://localhost:7094/weatherforecast');
}