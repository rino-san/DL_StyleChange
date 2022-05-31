using UnityEngine;
using Unity.Barracuda;  // import 必須

public class StyleChange : MonoBehaviour
{
    //  Barracuda 推論用
    public NNModel modelAsset;
    private Model m_RuntimeModel;
    private IWorker m_worker;

    public RenderTexture inputTexture;
    public RenderTexture outputTexture;

    // Start is called before the first frame update
    void Start()
    {
        // 学習済みモデルの読み込み
        m_RuntimeModel = ModelLoader.Load(modelAsset);

        // GPU 実行
        var workerType = WorkerFactory.Type.Compute;
        // CPU 実行
        // var workerType = WorkerFactory.Type.CSharp;

        // 推論エンジンの生成
        m_worker = WorkerFactory.CreateWorker(workerType, m_RuntimeModel);
    }

    private void Update()
    {
        // RenderTexture → Tensor に変換
        Tensor input = new Tensor(inputTexture);
        // 推論の実行
        m_worker.Execute(input);
        // 出力結果を取得
        Tensor output = m_worker.PeekOutput();
        // 出力結果（Tensor）を RenderTexture に保存
        // バッチサイズは 1 なので、第 2 引数は 0
        // チャンネル数は 3 なので、第 ３ 引数は 0 or 1 or 2
        // 0~255 → 0～1 にスケーリングするため、第 ４ 引数 は 1/255f、
        // 第 5 引数 のバイアスで、色味調整が可能。今回は 0 とする
        output.ToRenderTexture(outputTexture, 0, 0, 1 / 255f, 0, null);
        // メモリリークを回避のため、各ステップごとに Tensor は破棄
        // ※ PeekOutput で得られた Tensor の破棄は除く
        input.Dispose();
    }

    private void OnDestroy()
    {
        //終了時に 推論エンジン は破棄
        m_worker.Dispose();
    }

}