using UnityEngine;

public class PausarJuego : MonoBehaviour
{
    public bool juegoPausado = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            juegoPausado = !juegoPausado;
            GameManager.Instance.PausarJuego(juegoPausado);
        }
    }

    public void PausarBoton()
    {
        juegoPausado = !juegoPausado;
        GameManager.Instance.PausarJuego(juegoPausado);
    }
    public void ReanudarBoton()
    {
        juegoPausado = false;
        GameManager.Instance.PausarJuego(false);
    }

    public void GuardarBoton()
    {
        GameManager.Instance.BotonGuardar();
    }

    public void SalirBoton()
    {
        GameManager.Instance.BotonSalir();
    }
}
