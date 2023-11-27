namespace IC_Implementation;

class Program
{
    static void WriteArrayText(int[] message, int size) {

        Console.WriteLine();
        for (int i = size - 1; i >= 0; i--)
        {
            Console.Out.Write(message[i]);
        }
    }

    static int[] rotateRight(int rotations, int[] arr)
    {

        int[] newArray = new int[arr.Length];
        int size = arr.Length;

        for (int i = 0; i < rotations; i++)
        {
            newArray[size - (i + 1)] = arr[i];
        }

        for (int i = 0; i < size - rotations; i++)
        {
            newArray[i] = arr[i + rotations];
        }

        return newArray;
    }

    static int[] rotateLeft(int rotations, int[] arr) {

        int[] newArray = new int[arr.Length];
        int size = arr.Length;

        for (int i = 0; i < rotations; i++) {
            newArray[i] = arr[size - (i + 1)];
        }

        for (int i = size - 1; i >= rotations; i--) {
            newArray[i] = arr[i - 1];
        }

        return newArray;
    }

    static int[] getDerivatedKey(int day, int month) {

        day = (day > 31) ? day - 31 : day;
        day = (day < 1) ? day + 31 : day;

        month = (month > 12) ? month - 12 : month;
        month = (month < 1) ? month + 12 : month;

        int[] array = new int[8];

        int symetricNumber1 = month + day;
        int symetricNumber2 = (-month + 13) + (-day + 32);

        string s1Converted = symetricNumber1.ToString();
        string s2Converted = symetricNumber2.ToString();

        s1Converted = (symetricNumber1 < 10) ? ("0" + s1Converted) : s1Converted;
        s2Converted = (symetricNumber2 < 10) ? ("0" + s2Converted) : s2Converted;

        long concatenatedNumber = Convert.ToInt64(s1Converted + s2Converted);
        string binary = Convert.ToString(concatenatedNumber, 2);

        int[] arrayBits = binary.Select(c => int.Parse(c.ToString())).ToArray(); // Convertendo para array de inteiros
        Array.Copy(arrayBits, 0, array, 0, 8);

        return array;
    }

    static int[] GenMasterKey(int genSize, int day, int month, int rotationSize = 8)
    {
        int section = 0;

        int[] masterKey = new int[genSize];
        int[] rotationKey = new int[rotationSize];
        int[] derivatedKey = getDerivatedKey(day, month);

        int half = derivatedKey.Length / 2;
        int generatedBits = 0;
    
        // key expansion

        for (int i = 0; i < rotationSize; i++) {

            day++;
            month++;

            int[] bits = getDerivatedKey(day, month);
            rotationKey[i] = bits[i];
        }

        while (generatedBits < genSize) {

            // first Step getting a subkey
            int[] subKey = new int[half];

            for (int i = 0; i < half; i++) {
                int nextBit = derivatedKey[i] ^ derivatedKey[i + half];
                subKey[i] = nextBit;
            }

            // a sub chave e um vetor de 4 bits, gerados a partir da k_derivated, fazemos o ou exclusivo
            // dos 4 primeiros bits com os 4 ultimos bits.

            // second Step, key rotation
            for (int i = 0; i < rotationSize / 2; i++) {
                derivatedKey = rotateRight(rotationKey[i], derivatedKey);
                subKey = rotateRight(rotationKey[(rotationSize/2) + i], subKey);
            }

            // em seguida iremos rotacionar a chave derivada da seguinte forma pegaremos
            // a quantidade de bits 1 que das primeiras 4 posicoes da chave de rotacao
            // e entao iremos rotacionar para a direita essa quantidade de bits
            // e entao pegaremos a quantidade de bits 1 das 4 ultimas posicoes da chave de rotacao
            // e iremos rotacionar para essa quantidade de bits para a direita na sub chave



            // third Step Key transfer
            int copyElements= (generatedBits + subKey.Length < genSize) ? subKey.Length : genSize - generatedBits;

            Array.Copy(subKey, 0, masterKey, section*subKey.Length, copyElements);
            generatedBits += subKey.Length;
            section++;
        }

        return masterKey;
    }

    // if its not the same size than an error will be raised
    static int[] XORBitToBit(int[] arr1, int[] arr2) {

        int[] result = new int[arr1.Length];
        for (int i = 0; i < arr1.Length; i++) {
            result[i] = arr1[i] ^ arr2[i];
        }

        return result;
    }

    // this function can be better implemented, with other resources 
    static int[] RoundFunction(int[] bitArray, int[] key) {

        int half = key.Length / 2;

        int[] leftKey = new int[half];
        int[] rightKey = new int[half];

        Array.Copy(key, 0, leftKey, 0, half);
        Array.Copy(key, half, rightKey, 0, half);

        int[] f = XORBitToBit(bitArray, rightKey);
        int[] b = XORBitToBit(f, leftKey);

        return b;
    }


    static int[] FeistelCipher(int[] block, int[] key, int blockSize)
    {
        // it should have a verification for non multiple of 2

        int half = blockSize / 2;

        int[] left = new int[half];
        int[] right = new int[half];
        int[] newBlock = new int[blockSize];

        Array.Copy(block, 0, right, 0, half);
        Array.Copy(block, half, left, 0, half);

        left = XORBitToBit(RoundFunction( right, key ), left );

        for (int i = 0; i < half; i++) {
            newBlock[i] = left[i];
            newBlock[half + i] = right[i];
        }

        return newBlock;
    }

    static int[] FeistelDecipher(int[] block, int[] key, int blockSize) {

        int half = blockSize / 2;

        int[] left = new int[half];
        int[] right = new int[half];
        int[] newBlock = new int[blockSize];

        Array.Copy(block, 0, right, 0, half);
        Array.Copy(block, half, left, 0, half);

        int[] t = XORBitToBit(right, left);
        right = RoundFunction(t, key);

        for (int i = 0; i < half; i++)
        {
            newBlock[i] = left[i];
            newBlock[half + i] = right[i];
        }

        return newBlock;
    }

    static int[] EncryptEcb(int[] message, int[] masterKey, int rounds) {

        int[] key = (int[])masterKey.Clone();

        int blockQuantity = message.Length / key.Length;
        int keySize = key.Length;

        int[] encriptedMessage = new int[message.Length]; // if in the future the script expand the block this line will raise an exepcetion
        
        for (int i  = 0; i < blockQuantity; i++)
        {
            int blockStart = keySize*i;
            int[] block = new int[keySize];

            Array.Copy(message, blockStart, block, 0, keySize);

            for (int j = 0; j < rounds; j++) {

                block = FeistelCipher(block, key, keySize);
                block = rotateRight(1, block);
                key = rotateRight(1, key);
            }
            
            Array.Copy(block, 0, encriptedMessage, blockStart, keySize);
        }

        return encriptedMessage;
    }

    static int[] DecryptEcb(int[] cipher, int[] masterKey, int rounds) {


        int[] key = (int[]) masterKey.Clone();

        int blockQuantity = cipher.Length / key.Length;
        int keySize = key.Length;
        int[] message = new int[cipher.Length];

        // key sync
        if (rounds > 1) {
            for (int j = 0; j < rounds; j++)
            {
                key = rotateRight(1, key);
            }
        }

        for (int i = 0; i < blockQuantity; i++) {

            int blockStart = keySize * i;
            int[] block = new int[keySize];

            Array.Copy(cipher, blockStart, block, 0, keySize);
            
            for (int j = 0; j < rounds; j++) {
                block = rotateLeft(1, block);
                key = rotateLeft(1, key);
                block = FeistelDecipher(block, key, keySize);
            }

            Array.Copy(block, 0, message, blockStart, keySize);
        }

        return message;
    }

    static void Main(string[] args)
    {
        const int BLOCK_SIZE = 64;
        const int ROUNDS = 2;
        const int MESSAGE_SIZE = 128; 
        const int DEFAULT_VALUE = 0;

        int[] master_key = new int[BLOCK_SIZE];
        int[] message = new int[MESSAGE_SIZE];

        // ! not oficial implementation on the future this should be load by files
        // on future message_size will not be necessary, missing information will be add on the last block
        // the implementation bellow its only for test to generate exact 2 blocks

        // message example
        string _message = "0100101111011011001101010110100111100010110111001011010111101001101110010110101111010001101010110100111101001111000101101110";

        int day = 1;
        int month = 1;

        master_key = GenMasterKey(BLOCK_SIZE, day, month);

        for (int i = 0; i < MESSAGE_SIZE; i++) {
            try
            {
                message[i] = (_message[i] == '0') ? 0 : 1; 
            }
            catch {
                message[i] = DEFAULT_VALUE;
            }
        }   

        int[] encriptedMessage = EncryptEcb(message, master_key, ROUNDS);
        int[] decriptedMessage = DecryptEcb(encriptedMessage, master_key, ROUNDS);

        Console.WriteLine("---------------");
        Console.Write("\nMessage: ");
        WriteArrayText(message, MESSAGE_SIZE);

        Console.Write("\nKey: ");
        WriteArrayText(master_key, BLOCK_SIZE);

        Console.Write("\nEncripted Message");
        WriteArrayText(encriptedMessage, MESSAGE_SIZE);

        Console.Write("\nDecripted Message");
        WriteArrayText(decriptedMessage, MESSAGE_SIZE);



    }
}
