import { useEffect, useState } from 'react';
import axiosClient from '../api/axiosClient';

export default function PersonelList() {
    const [data, setData] = useState([]);

    useEffect(() => {
      axiosClient.get('/Personel/All')
      .then(res=> SetData(res.data))
      .catch(()=> console.log("Error fetching data"))  
    }, []);


    return (
        <div>
            <h2>Personel Listesi</h2>
            <ul>
                {data.map(p => (
                    <li key={p.id}> {p.ad} {p.soyad} - {p.tcKimlikNo}</li>
                ))}
            </ul>
        </div>
    );
}