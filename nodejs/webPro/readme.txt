Express ��һ���������� node.js WebӦ�ÿ��:

app.js����Ŀ����ļ���package.json��npm �������ļ���bin�ļ��������www.js��һЩȫ���������Լ����������õȡ�
public �ļ��������������Ŀ��̬�ļ�Ŀ¼��js,css�Լ�ͼƬ��
routes�ļ������������·�ɼ����Ĵ�������ļ���
views�ļ����������ģ���ļ�

npm install
npm start  ����������

2��Ŀ¼�ṹ

bin������������г���
node_modules����������е���Ŀ�����⡣
public������ž�̬�ļ�������css��js��img�ȡ�
routes�������·���ļ���
views�������ҳ���ļ���ejsģ�壩��
app.js�������������ļ���
package.json������Ŀ�������ü���������Ϣ��

routes �ļ��д��·��js�ļ����൱�ڿ�������������֯չʾ������ http://localhost:3000/users
router.get('/', function(req, res, next) {
  
   res.render('index', { title: 'Express' }); //����ģ�� views/index
});

public �ļ��д���Դ�ļ� ���� http://localhost:3000/images/rank.png