import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

class LightArray
{
    List<String> valid;
    List<String> alt;
    List<String> av;
    
    public static void main(String[] args)
    {
        LightArray ins = new LightArray();
        while(ins.valid.size() < 8)
            ins.solve();
        ins.print();
    }

    LightArray()
    {
        valid = new ArrayList<String>();
        alt = new ArrayList<String>();
        av = new ArrayList<String>();

        for(int i = 0; i < 64; i++)
        {
            String s = Integer.toBinaryString(i);
            while(s.length() < 6)
                s = '0' + s;
                av.add(s);
        }
    }

    void solve()
    {
        valid = new ArrayList<String>();
        alt = new ArrayList<String>();
        Collections.shuffle(av);

        for(int i = 0; i < av.size(); i++)
        {
            if(alt.contains(av.get(i)))
                continue;
            
            List<String> perm = new ArrayList<String>();
            
            for(int j = 0; j < 6; j++)
            {
                String temp = new String(av.get(i));
                char[] tempChars = temp.toCharArray();
                tempChars[j] = (tempChars[j] == '0' ? '1' : '0');
                temp = String.valueOf(tempChars);

                if(valid.contains(temp) || alt.contains(temp))
                    break;

                perm.add(temp);
            }

            if(perm.size() != 6)
                continue;

            valid.add(av.get(i));
            for (String p : perm) {
                alt.add(p);
            }

        }

    }

    void print()
    {
        for (String set : valid) {
            System.out.println(set);
        }
    }
}