package org.mjapp;

import java.io.IOException;

import android.app.NativeActivity;
import android.content.res.AssetFileDescriptor;
import android.os.Bundle;
import android.util.Log;
import android.media.MediaPlayer;
//import org.fmod.FMODAudioDevice;

public class MJAppActivity extends NativeActivity {
	
	private static final String TAG = "MJAppActivity";
	private MediaPlayer player = null;
	
	//private FMODAudioDevice mFMODAudioDevice = new FMODAudioDevice();
	
    @Override  
    protected void onCreate(Bundle savedInstanceState) {  
        super.onCreate(savedInstanceState);
        Log.v(TAG, "onCreate()");
    }
    
    @Override
    public void onStart()
    {
    	super.onStart();   	
    	//mFMODAudioDevice.start();
    	playBgMusic("sound1.mid", true);
    	Log.v(TAG, "onStart()");
    }
    
    @Override
    public void onStop()
    {
    	//mFMODAudioDevice.stop();
    	super.onStop();
    	Log.v(TAG, "onStop()");
    }
    
    public void playBgMusic(String assetFileName, Boolean looping)
    {
    	AssetFileDescriptor afd;
		try {
			afd = getAssets().openFd(assetFileName);
			player = new MediaPlayer();
	        player.setDataSource(afd.getFileDescriptor(),afd.getStartOffset(),afd.getLength());
	        player.prepare();
	        player.setLooping(looping);
	        player.start();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
       
    }
}
